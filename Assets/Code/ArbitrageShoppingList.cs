using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


public partial class Arbitrage
{
    //Creates an optimal list of goods to buy to exploit an arbitrage, given 
    //the details of the journey that change the profitability of a product. 

    //The considerations include

    //Whether the products will "fit" onto our "vehicle" (both of these are 
    //abstract ideas. For example, we can consider our budget a vehicle with
    //limited financial space),

    //What variable costs may arise from transporting a product (beyond the 
    //purchase cost), and

    //If we have a target metric (like a target total mass, or budget), we can
    //use the journey's fixed costs to make a better prediction of a product's 
    //profitability by spreading the fixed costs over the cargo.

    //"fill_space" and "constraint_spaces" represent that first point. Note 
    //that they are considered to be orthogonal. F.e. putting a widget on board
    //simultaniously takes up literal volume in the hold, while also taking up
    //an abstract "volume" of the budget available to buy products, as well as
    //possibly reducing the remaining mass that can be packed without changing 
    //the performance characteristics of the craft. 
    
    public Manifest MakeShoppingList(
        Manifest products,
        Func<Item, float, float> GetTransportCosts = null,
        Space fill_space = null,
        float fixed_costs = 0,
        IEnumerable<Space> constraint_spaces = null,
        Func<Item, bool> IsProductLegal = null)
    {
        //Input sanitizing

        if (GetTransportCosts == null)
            GetTransportCosts = (product, quantity) => 0;

        if (constraint_spaces == null)
            constraint_spaces = Enumerable.Empty<Space>();

        if (IsProductLegal == null)
            IsProductLegal = product => true;


        //Functions

        Manifest shopping_list = new Manifest();

        Action<Item, float> AddToShoppingList = (product, quantity) =>
        {
            shopping_list.Add(product, quantity);

            fill_space.Pack(product, quantity);
            foreach (Space space in constraint_spaces)
                space.Pack(product, quantity);
        };


        //The "marginal quantity" of an item is the maximum additional amount 
        //you can acquire without experiencing significant nonlinearity - 
        //that is, the first unit acquired has a different density than the 
        //last unit. 

        //F.e. when purchasing a product, the ratio of mass to cost changes as
        //you buy more - you will have to pay more, and thus its 
        //"financial density" goes down. This prevents accurately estimating a
        //product's profitability. 

        Func<Item, float> CalculateMarginalQuantity = product =>
        {
            float marginal_quantity = products[product] -
                                      shopping_list[product];

            if (fill_space != null)
                marginal_quantity =
                    Mathf.Min(marginal_quantity,
                              fill_space.GetMarginalQuantity(product));

            foreach (Space constraint_space in constraint_spaces)
                marginal_quantity =
                    Mathf.Min(marginal_quantity,
                              constraint_space.GetMarginalQuantity(product));

            return marginal_quantity;
        };

        Dictionary<Item, float> marginal_quantities = null;
        Action BakeMarginalQuantities = () =>
        {
            marginal_quantities = 
                products.Samples.ToDictionary(product => product, 
                                            CalculateMarginalQuantity);
        };
        BakeMarginalQuantities();

        Func<Item, float> GetMarginalQuantity = product =>
        { return marginal_quantities[product]; };


        //Estimates how many units of product will fit into the remaining 
        //volume available in the Space prodided, using the product's next 
        //marginal quantity's density as a guide. 

        Func<Item, Space, float> GetProjectedCapacity = (product, space) =>
        {
            float marginal_quantity = GetMarginalQuantity(product);

            return marginal_quantity / 
                   space.GetCapacityFraction(product, marginal_quantity);
        };


        //Determines if a product may prevent complete filling of fill_space
        //by seeing whether the quantity necessary for fill_space is greater
        //than the remaining volume available in the constraint_spaces.

        //This function does not directly compare capacity of fill space to 
        //constraint space because we are only interested whether at the 
        //_current_rate_ the product is projected to outrun the volume 
        //available. This prevents unecessary culling when dealing with 
        //supra-linear spaces. 

        //f.e. the first widget bought may cost $1, which does not endanger 
        //going over budget. But the last widget may cost $1T, which does.

        //Note however, that this could be suboptimal in cases of 
        //sub-linear spaces. 

        Func<Item, bool> IsDense = product =>
        {
            float fill_quantity = GetProjectedCapacity(product, fill_space);

            foreach (Space constraint_space in constraint_spaces)
                if (GetProjectedCapacity(product, constraint_space) < fill_quantity)
                    return false;

            return true;
        };


        Func<Item, float, float> GetVariableCosts = (product, quantity) =>
        {
            return  GetPurchaseCost(product, quantity) +
                    GetTransportCosts(product, quantity);
        };

        Func<Item, bool> IsProductProfitable = product =>
        {
            float marginal_quantity = GetMarginalQuantity(product);

            return GetSaleValue(product, marginal_quantity) > 
                   GetVariableCosts(product, marginal_quantity);
        };

        Func<Item, float> GetProductROI = 
        (product) =>
        {
            float quantity = GetMarginalQuantity(product);
            float existing_quantity = shopping_list[product];

            float variable_costs =
                GetVariableCosts(product, existing_quantity + quantity) -
                GetVariableCosts(product, existing_quantity);
            float total_costs = variable_costs;
            if(fill_space != null)
                total_costs += fixed_costs * 
                               fill_space.GetCapacityFraction(product, quantity);

            float sale_value =
                GetSaleValue(product, existing_quantity + quantity) -
                GetSaleValue(product, existing_quantity);

            return (sale_value - total_costs) /
                    total_costs;
        };

        Action<Manifest> MarginalizePackage = package =>
        {
            Dictionary<Item, float> fractions = 
                package.Samples.ToDictionary(
                    product => product, 
                    product => GetMarginalQuantity(product) /
                               package[product]);

            Item most_marginal_product = package.Samples
                .MinElement(product => fractions[product]);
        };

        Func<Manifest, float> GetPackageROI = package =>
        {
            float variable_costs = 0;
            float sale_value = 0;

            foreach (Item product in package.Samples)
            {
                float quantity = package[product];
                float existing_quantity = shopping_list[product];

                variable_costs +=
                    GetVariableCosts(product, quantity + existing_quantity) -
                    GetVariableCosts(product, existing_quantity);

                sale_value +=
                    GetSaleValue(product, quantity + existing_quantity) -
                    GetSaleValue(product, existing_quantity);
            }

            return (sale_value - variable_costs) /
                    variable_costs;
        };

        Func<List<Item>> GetLegalProducts = () =>
        {
            return products.Samples
                .Where(IsProductLegal)
                .Where(product => GetMarginalQuantity(product) > 0.000001f)
                .Where(IsProductProfitable)
                .ToList();
        };
        List<Item> legal_products = GetLegalProducts();


        //While there are still products available, pick the best product 
        //(measured by roi) and add a small amount of it to the shopping list.
        //(products are filtered if they won't fit or aren't profitable)

        //Shopping list is created such that the fill_space (if provided) has 
        //as little remaining room inside it as possible. Furthermore, none of
        //the Spaces are allowed to be overfilled. If any space has no 
        //remaining room for more products, the shopping list is finished. 

        //Consequentally, in order to avoid returning an underfilled shopping
        //list, if the best product would overfill a constraint space if you 
        //had enough of it to fill up the "fill_space" (such a product is 
        //considered to not be "dense"), then this tries to find a combination
        //of products (a "package") that _is_ dense while maximizing quantity
        //of the best product and beating the best dense product.

        while (legal_products.Count > 0)
        {
            legal_products.Sort(product => -GetProductROI(product));

            List<Item> dense_products = null;
            if (fill_space != null)
                dense_products = legal_products
                    .Where(IsDense)
                    .ToList()
                    .GetRangeOrGetClose(0, 10);


            //If best_product is dense, just add it to the shopping_list. If 
            //not, try to create a package that includes the best_product while
            //being dense (by including a dense product to compensate). If you
            //fail, make next best product the new best_product and try again.

            //Package Construction: 

            //Compute ratio of GetMarginalQuantity to GetCapacityFraction and 
            //use it to guess how much of each component product is required to
            //fill up the fill_space and the constraint_space. From here, its a
            //simple linear mixing of the two components such that the 
            //fill_space and the constraint space are both full. This is 
            //repeated for all constraint spaces and the smallest package is 
            //selected. Finally, marginalize the package. 

            //This algorithm assumes linearity to perform the mixing, however, 
            //because we use GetMarginalQuantity, linearity is ensured over 
            //the quantities we are using. And because the package is 
            //marginalized at the end, these assumptions hold. 

            //This algorithm assumes that components are in direct competition 
            //for space, which may not be the case (f.e. a liquid and a solid).
            //This may cause suboptimality.

            Item best_product = legal_products.First();
            Manifest package = null;

            while (fill_space != null &&
                   dense_products.Count != 0 &&
                   !dense_products.Contains(best_product) &&
                   package == null)
            {
                legal_products.Remove(best_product);

                foreach (Item dense_product in dense_products)
                {
                    IEnumerable<Space> constraining_spaces = constraint_spaces
                        .Where(constraint_space =>
                            GetProjectedCapacity(best_product, fill_space) > 
                            GetProjectedCapacity(best_product, constraint_space));

                    Manifest candidate_package = constraining_spaces
                    .Select(constraint_space =>
                    {
                        float af = GetProjectedCapacity(best_product, fill_space);
                        float ac = GetProjectedCapacity(best_product, constraint_space);

                        float bf = GetProjectedCapacity(dense_product, fill_space);
                        float bc = GetProjectedCapacity(dense_product, constraint_space);

                        float a_hybrid = (1 - bf / bc) /
                                         (1 / ac - bf / bc / af);
                        float b_hybrid = bf * (1 - a_hybrid / af);

                        return new Manifest(
                            new Dictionary<Item, float>{
                                { best_product, a_hybrid },
                                { dense_product, b_hybrid } });
                    }).MinElement(package_ => package_[best_product]);
                    MarginalizePackage(candidate_package);

                    if (GetPackageROI(candidate_package) >
                        GetProductROI(dense_products.First()))
                    {
                        package = candidate_package;
                        break;
                    }
                }

                best_product = legal_products.First();
            }

            if(package == null)
                AddToShoppingList(best_product,
                                  GetMarginalQuantity(best_product));
            else
                foreach (Item product in package.Samples)
                    AddToShoppingList(product, package[product]);


            BakeMarginalQuantities();
            legal_products = GetLegalProducts();
        }

        return shopping_list;
    }


    //Creates a shopping list where one of the constraints is that products 
    //must fit within a given Storage volume.
    //Optional parameter "junk" describes products which are presupposed to be 
    //on board and thus reduce available space. 

    public Manifest MakeShoppingList(
            Manifest products,
            Func<Item, float, float> GetTransportCosts,
            Storage storage, Manifest junk,
            LinearSpace fill_space = null,
            float fixed_costs = 0,
            IEnumerable <Space> constraint_spaces = null,
            Func<Item, bool> CanTrade = null)
    {
        if (constraint_spaces == null)
            constraint_spaces = Enumerable.Empty<Space>();
        constraint_spaces = constraint_spaces.Append(
            CreateStorageSpace(storage, junk));

        return MakeShoppingList(
            products,
            GetTransportCosts,
            fill_space, fixed_costs,
            constraint_spaces,
            CanTrade);
    }


    //Creates a shopping list where one of the constraints is that the 
    //total cost of all products must not exist a given budget. 

    public Manifest MakeShoppingList(
        Manifest products,
        Func<Item, float, float> GetTransportCosts,
        Market market, float budget,
        LinearSpace fill_space = null,
        float fixed_costs = 0,
        IEnumerable<Space> constraint_spaces = null,
        Func<Item, bool> CanTrade = null)
    {
        if (constraint_spaces == null)
            constraint_spaces = Enumerable.Empty<Space>();
        constraint_spaces = constraint_spaces.Append(CreateBudgetSpace(
            budget,
            market,
            (product, quantity) => GetTransportCosts(product, quantity)));

        return MakeShoppingList(
            products,
            GetTransportCosts,
            fill_space, fixed_costs,
            constraint_spaces,
            CanTrade);
    }


    //This combines the features of the above two methods.

    public Manifest MakeShoppingList(
        Manifest products,
        Func<Item, float, float> GetTransportCosts,
        Storage storage, Manifest junk,
        Market market, float budget,
        LinearSpace fill_space = null,
        float fixed_costs = 0,
        IEnumerable<Space> constraint_spaces = null,
        Func<Item, bool> CanTrade = null)
    {
        if (constraint_spaces == null)
            constraint_spaces = Enumerable.Empty<Space>();
        constraint_spaces = constraint_spaces.Append(
            CreateStorageSpace(storage, junk));

        return MakeShoppingList(
            products,
            GetTransportCosts,
            market, budget,
            fill_space, fixed_costs,
            constraint_spaces,
            CanTrade);
    }


    //Creates a function that computes the additional cost of products that 
    //appear in the list of provisions. (Buying provisions may inflate the 
    //price of a product)

    public Func<Item, float, float> CreateTransportCostFunction(Manifest provisions)
    {
        return (product, quantity) =>
        {
            if (!provisions.Contains(product))
                return 0;

            float provision_quantity = provisions[product];

            return GetPurchaseCost(product, provision_quantity + quantity) -
                   GetPurchaseCost(product, provision_quantity) -
                   GetPurchaseCost(product, quantity);
        };
    }
}
