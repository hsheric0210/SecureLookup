using SecureLookup.Parameter;

namespace SecureLookup;

public class ProgramParameter
{
	[ParameterAlias("db")]
	[ParameterDescription("The database file to use")]
	[MandatoryParameter]
	public string Database { get; set; } = "";

	[ParameterAlias("pass", "psw", "pw")]
	[ParameterDescription("The password to open the database")]
	[MandatoryParameter]
	public string Password { get; set; } = "";

	[ParameterAlias("cmd")]
	[ParameterDescription("The command that will run immediately after database loaded")]
	public string? Command { get; set; }

	[ParameterAlias("noloop", "nl")]
	[ParameterDescription("Disable the main loop. The program will immediately exit after executing the command specified by '-command' parameter.")]
	public bool DisableLoop { get; set; }

	[ParameterAlias("batch")]
	[ParameterDescription("Execute each lines of specified file as command AND EXIT. Remember to append 'save' at the last line to save all changes.")]
	public string? BatchFile { get; set; }

	[ParameterAlias("export")]
	[ParameterDescription("Export all entries to specified file AND EXIT.")]
	public string? ExportFile { get; set; }
}
