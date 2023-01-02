using SecureLookup.Db;
using SecureLookup.Parameter;
using System.Text;
using System.Text.RegularExpressions;

namespace SecureLookup.Commands;
internal class ChangePasswordCommandParameter
{
	[ParameterAlias("pass", "psw", "pw", "p")]
	[ParameterDescription("The new database password; only following characters allowed: " + ChangePasswordCommand.AllowedChars)]
	[MandatoryParameter]
	public string Password { get; set; } = "";

	[ParameterAlias("encryption", "enc")]
	[ParameterDescription("New database encryption algorithm")]
	public string? EncryptionAlgorithm { get; set; }
}

internal class ChangePasswordCommand : AbstractCommand
{
	internal const string AllowedChars = "A-Z a-z 0-9 !#$%&'()*+,-./:;<=>?@[]^`{|}~";

	public override string Description => "Changes the database password.";

	public override string HelpMessage => ParameterDeserializer.GetHelpMessage<ChangePasswordCommandParameter>();


	public ChangePasswordCommand(Program instance) : base(instance, "changepassword")
	{
	}

	protected override bool Execute(string[] args)
	{
		if (!ParameterDeserializer.TryParse(out ChangePasswordCommandParameter param, args))
			return false;

		var newPassword = param.Password;
		if (Regex.IsMatch(newPassword, "[^\\w!#$%&'()*+,-./:;<=>?@\\[\\]^`{|}~]"))
		{
			Console.WriteLine("Password contains unsupported characters.");
			Console.WriteLine("Allowed characters: " + AllowedChars);
		}
		Instance.Database.ChangePassword(Encoding.UTF8.GetBytes(newPassword), param.EncryptionAlgorithm);
		return true;
	}
}
