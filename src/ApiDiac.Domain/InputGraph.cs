namespace ApiDiac.Domain
{
    using System.ComponentModel;

    public class InputGraph
    {
        [DefaultValue("http://your_default_dataset")]
        public Uri DatasetName { get; set; }

        public string content { get; set; }
    }
}
