using System.Xml;

namespace SecureLookup.Commands;
internal class ChangePasswordCommand : AbstractCommand
{
	public ChangePasswordCommand(Program instance) : base(instance, "newdbpass")
	{
	}

	protected override string ParameterExplain => @"
  Mandatory parameters:
	-pass<password>			- New database password";


	protected override bool Execute(string[] args)
	{
		var pass = args.GetSwitch("pass");
		if (string.IsNullOrWhiteSpace(pass))
			return false;


		return true;
	}
}
