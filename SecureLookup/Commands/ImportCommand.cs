﻿namespace SecureLookup.Commands;
internal class ImportCommand : AbstractCommand
{
	public ImportCommand(Program instance) : base(instance, "import")
	{
	}

	protected override string ParameterExplain => "<batchList_filePath> [separator]";

	protected override bool Execute(string[] args) => throw new NotImplementedException();
}