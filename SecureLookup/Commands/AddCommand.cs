using System.Xml;

namespace SecureLookup.Commands;
internal class AddCommand : AbstractCommand
{
	public AddCommand(Program instance) : base(instance, "add")
	{
	}

	protected override string Usage => "<name> <filePath> <filePassword> [description] [nameDictionary]";

	protected override int MandatoryParameterCount => 3;

	protected override bool Execute(string[] args)
	{
		string name = args[0];
		string filePath = args[1];
		string password = args[2];
		string description = "";
		string nameDictionary = "";
		if (args.Length > 3)
		{
			description = args[3];
			if (args.Length > 4)
				nameDictionary = args[4];
		}

		XmlElement node = Instance.Db.CreateElement("archive");
		node.SetAttribute("name", name);
		node.SetAttribute("filePath", filePath);
		node.SetAttribute("password", password);
		node.InnerText = description;
		Instance.Db.DocumentElement!.AppendChild(node);
		Instance.SaveDb();
		return true;
	}
}
