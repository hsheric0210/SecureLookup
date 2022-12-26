namespace SecureLookup.Commands;
internal class FindCommand : AbstractCommand
{
	public FindCommand(Program instance) : base(instance, "find")
	{
	}

	protected override string Usage => @"
  Mandatory parameters:
	-f<(e)quals/(c)ontains/(s)tartsWith/(e)ndsWith/(r)egex>
	-group<(a)ll/(n)ame/(i)d/(f)ileName/>		- Search
	-kw<keyword/regular>		- Keyword to search";

	protected override int MandatoryParameterCount => 2;

	protected override bool Execute(string[] args)
	{
		Instance.Db.Entries.FindAll()
		return true;
	}
}
