namespace SecureLookup.Commands;
internal class ClearCacheCommand : AbstractCommand
{
	public override string HelpMessage => "";

	public override string Description => "Clears generated name cache.";

	public ClearCacheCommand(Program instance) : base(instance, "clearcache")
	{
	}

	protected override bool Execute(string[] args)
	{
		Instance.Db.GeneratedFileNames.Clear();
		Instance.EncryptedDb.MarkDirty();
		return true;
	}
}
