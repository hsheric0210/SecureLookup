using System.Text.RegularExpressions;
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

		if (Regex.IsMatch(pass, "[^\\w!#$%&'()*+,-./:;<=>?@\\[\\]^`{|}~]"))
		{
			Console.WriteLine("Password contains unsupported characters.");
			Console.WriteLine("Allowed characters: A-Z a-z 0-9 !#$%&'()*+,-./:;<=>?@[]^`{|}~");
		}
		Instance.ChangePassword(pass);
		return true;
	}
}
