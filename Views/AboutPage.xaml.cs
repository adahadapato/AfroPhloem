using System.Collections.ObjectModel;
using AfroPhloem.Models;

namespace AfroPhloem.Views;

public partial class AboutPage : ContentPage
{
    public ObservableCollection<FeatureComparison> Features { get; } = new();

    public AboutPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadFeatures();
    }

    private void LoadFeatures()
    {
        Features.Add(new FeatureComparison
        {
            Icon = "🔥",
            Title = "Cross-Border African Food Trade",
            ExistingPlatformText = "Delivers only within the same country. No international food shipping. No diaspora-focused logistics.",
            AfroPhloemText = "Send African food from Ghana or Nigeria to the UK — dry food, packaged items, and bulk diaspora orders.",
            ValueAdded = "You're not just ordering dinner — you're connecting Africa to its diaspora."
        });

        Features.Add(new FeatureComparison
        {
            Icon = "🤝",
            Title = "Seller Empowerment",
            ExistingPlatformText = "Focuses on registered restaurants only. Small home vendors are excluded. High commissions, low control.",
            AfroPhloemText = "Home cooks, small food businesses, market women, and export-ready vendors can all build a profile and grow a brand.",
            ValueAdded = "You're creating digital livelihoods, not just deliveries."
        });

        Features.Add(new FeatureComparison
        {
            Icon = "🌍",
            Title = "Cultural & Local Relevance",
            ExistingPlatformText = "Generic categories, Western-centric UX, little cultural context.",
            AfroPhloemText = "Built by Africans, for African food — local meals by name, region-specific dishes, and UX that respects language and food customs.",
            ValueAdded = "This isn't African food on a European app — it's African food at the centre."
        });

        Features.Add(new FeatureComparison
        {
            Icon = "⚡",
            Title = "One Platform, Three Markets",
            ExistingPlatformText = "Country-by-country silos. No shared ecosystem.",
            AfroPhloemText = "A single platform operating in Ghana, Nigeria, and the UK — same account, same brand, built for expansion to the EU, US, and Canada.",
            ValueAdded = "Scalable pan-African food infrastructure, not just an app."
        });

        Features.Add(new FeatureComparison
        {
            Icon = "🛒",
            Title = "Marketplace + Logistics + Community",
            ExistingPlatformText = "Delivery only. No relationship between buyer and seller.",
            AfroPhloemText = "A marketplace, local and international logistics, and a community of trusted vendors and repeat buyers.",
            ValueAdded = "Trust, repeat business, and brand loyalty."
        });

        Features.Add(new FeatureComparison
        {
            Icon = "💛",
            Title = "Diaspora-Focused Problem Solving",
            ExistingPlatformText = "Not built for migrants or diaspora needs.",
            AfroPhloemText = "Solves real diaspora problems — sending food to family, receiving food from home, and supporting home businesses from abroad.",
            ValueAdded = "Emotional and cultural value, not just convenience."
        });
    }
}
