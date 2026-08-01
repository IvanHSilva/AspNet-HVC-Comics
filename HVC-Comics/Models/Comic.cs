namespace HVC_Comics.Models;

public class Comic
{
    // Fields
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
    public int Stories { get; set; }
    public int Articles { get; set; }

    public string ComicMonth { get; set; } = string.Empty;
    public int ComicYear { get; set; }
    public DateOnly ComicDate { get; set; }
    public int Pages { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public string Licensor { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;
    public string Coin { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string ComicSituation { get; set; } = string.Empty;
    public string PaperType { get; set; } = string.Empty;
    public string Binding { get; set; } = string.Empty;
    public string CoverType { get; set; } = string.Empty;

    public string CoverChar { get; set; } = string.Empty;
    public string ComicTitle { get; set; } = string.Empty;
    public string ComicCall { get; set; } = string.Empty;
    public string ComicCover { get; set; } = string.Empty;
    public int ComicNumber { get; set; }

    public string Period { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string Conservation { get; set; } = string.Empty;
    public string Problem1 { get; set; } = string.Empty;
    public string Problem2 { get; set; } = string.Empty;
    public DateOnly RegDate { get; set; }

    public bool IsLastEdition { get; set; }
    public bool HaveMail { get; set; }
    public bool HaveChecklist { get; set; }
    public bool IsBook { get; set; }
    public bool IsReedition { get; set; }
    public bool IsCrossover { get; set; }
    public bool IsPhisic { get; set; }
    public bool IsDigital { get; set; }
    public bool IsBlackWithe { get; set; }


    public string RegServer { get; set; } = string.Empty;
}
