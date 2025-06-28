namespace web_app_template.Domain.Models
{
    public class PropertyFilter
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public string Operator { get; set; } = "=="; // "==", "!=", ">", "<", ">=", "<="
    }
}
