namespace Danjin.Extensions;

public static class StringExtensions
{
	private const string ResRoot = "res://Danjin/Images/";

	public static string ImagePath(this string path)
	{
		return "res://Danjin/Images/" + path;
	}

	public static string CardImagePath(this string path)
	{
		return "res://Danjin/Images/Cards/" + path;
	}

	public static string BigCardImagePath(this string path)
	{
		return "res://Danjin/Images/Cards/Big/" + path;
	}

	public static string PowerImagePath(this string path)
	{
		return "res://Danjin/Images/Powers/" + path;
	}

	public static string BigPowerImagePath(this string path)
	{
		return "res://Danjin/Images/Powers/Big/" + path;
	}

	public static string RelicImagePath(this string path)
	{
		return "res://Danjin/Images/Relics/" + path;
	}

	public static string BigRelicImagePath(this string path)
	{
		return "res://Danjin/Images/Relics/Big/" + path;
	}

	public static string CharacterUiPath(this string path)
	{
		return "res://Danjin/Images/Charui/" + path;
	}

	public static string PotionImagePath(this string path)
	{
		return "res://Danjin/Images/Potions/" + path;
	}
}
