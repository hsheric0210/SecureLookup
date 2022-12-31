namespace SecureLookup;
public static class ConsoleUtils
{
	public static bool CheckContinue(string? filler = "")
	{
		var answer = "";
		while (answer is not "Y" and not "N")
		{
			Console.Write((filler is null ? "" : filler + " ") + "Do you want to continue? [Y/N]: ");
			var line = Console.ReadLine();
			answer = line?.ToUpperInvariant();
		}
		return answer == "Y";
	}
}
