namespace ApiDiac.Domain
{
    using System.ComponentModel;

    public class InputDataConceptAndId
    {
        [DefaultValue("http://your_default_concept")]
        public Uri Concept { get; set; }

        [DefaultValue("http://your_default_id")]
        public Uri Id { get; set; }
    }
}
