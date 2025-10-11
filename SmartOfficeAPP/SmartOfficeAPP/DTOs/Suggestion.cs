namespace SmartOfficeAPP.DTOs
{
    public class Suggestion
    {
        public int Id { get; set; }
        public int UtilisateurId { get; set; }
        public string ContenuSuggestion { get; set; }
        public DateTime DateCreation
        {
            get; set;
        }

    }
}
