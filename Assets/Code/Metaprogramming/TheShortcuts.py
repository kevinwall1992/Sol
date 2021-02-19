import re

properties = []

pattern = re.compile("\s*public\s*([a-zA-Z0-9_]+)\s*([a-zA-Z0-9_]+)\s*;\s*")

for line in open("../Scene.cs", "r").readlines():
  match = pattern.match(line);
  
  if match is not None:
    type = match.group(1)
    name = match.group(2)

    properties.append("    public static " + type + " " + name + " { get { return Scene." + name + "; } }\n")


output = ""

for line in open("../The.cs", "r").readlines():
  if line not in properties:
    output += line

  if re.match(".*//TheShortcuts\.py.*\n", line):
    output += "\n" + "".join(properties)

open("../The.cs", "w").writelines(output)

