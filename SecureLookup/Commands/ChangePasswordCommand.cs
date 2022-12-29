using System.Text.RegularExpressions;

namespace SecureLookup.Commands;
internal class ChangePasswordCommandParameter
{
	[ParameterAlias("pass", "psw", "pw", "p")]
	[ParameterDescription($"The new database password; only '${ChangePasswordCommand.AllowedChars}' characters allowed.")]
	[MandatoryParameter]
	public string Password { get; set; } = "";
}

internal class ChangePasswordCommand : AbstractCommand
{
	internal const string AllowedChars = "A-Z a-z 0-9 !#$%&'()*+,-./:;<=>?@[]^`{|}~";

	public override string Description => "Changes the database password.";

	public override string HelpMessage => ParameterSerializer.GetHelpMessage<ChangePasswordCommand>();


	public ChangePasswordCommand(Program instance) : base(instance, "changepassword")
	{
	}

	protected override bool Execute(string[] args)
	{
		if (!ParameterSerializer.TryParse(out ChangePasswordCommandParameter param, args))
			return false;

		var newPassword = param.Password;
		if (Regex.IsMatch(newPassword, "[^\\w!#$%&'()*+,-./:;<=>?@\\[\\]^`{|}~]"))
		{
			Console.WriteLine("Password contains unsupported characters.");
			Console.WriteLine("Allowed characters: " + AllowedChars);
		}
		Instance.ChangePassword(newPassword);
		return true;
	}
}
