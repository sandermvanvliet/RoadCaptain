namespace RoadCaptain.DataExtractor
{
    public enum WadAssetType : uint
    {
        GDE,
        SKY,
        COLL,
        BOG,
        SND,
        ENTITY,
        MOBY,
        TIE,
        SHRUB,
        TEXTURE,
        SHADER,
        PARTICLE,
        UI,
        GLOBAL,
        NAV,
        PVAR_INCLUDE,
        TUNING_INCLUDE,
        CNT
    };

    public class AssetUtils
    {
        private static readonly string[] AssetType =
        {
            "gde", "sky", "coll", "bog", "snd", "entity", "moby",
            "tie", "shrub", "texture", "shader", "particle", "ui", "global", "nav", "pvar_include",
            "tuning_include", "???"
        };
        
        public static string GetAssetTypeName(int  assetType)
        {
            if (assetType < 0 || assetType >= AssetType.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(assetType));
            }
            
            return AssetType[assetType];
        }
        
        public static WadAssetType GuessAssetType(string filePath)
        {
            // C# strings aren't null-terminated in the same way, 
            // so we check for null or empty first.
            if (string.IsNullOrEmpty(filePath)) 
            {
                return WadAssetType.GLOBAL;
            }

            // Convert to lowercase if you want the check to be case-insensitive
            // string path = filePath.ToLower(); 

            if (filePath.EndsWith(".gde"))
            {
                return WadAssetType.GDE;
            }
    
            if (filePath.EndsWith(".ztx") || filePath.EndsWith(".tgax"))
            {
                return WadAssetType.TEXTURE;
            }
    
            if (filePath.EndsWith(".sh"))
            {
                return WadAssetType.SHADER;
            }

            return WadAssetType.GLOBAL;
        }
    }
}