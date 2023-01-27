namespace ApiDiac.Domain
{
    using System.ComponentModel;

    public class InputDataProfile
    {
        [DefaultValue("http://your_default_profile")]
        public Uri Profile { get; set; }
    }
}
