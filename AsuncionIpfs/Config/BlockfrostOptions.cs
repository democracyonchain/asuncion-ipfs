namespace AsuncionIpfs.Config
{
    public class BlockfrostOptions
    {
        public string Network { get; set; } = "preview";
        public BlockfrostCardanoOptions Cardano { get; set; } = new();
        public BlockfrostIpfsOptions Ipfs { get; set; } = new();
    }

    public class BlockfrostCardanoOptions
    {
        public string ApiKey { get; set; } = "";
    }

    public class BlockfrostIpfsOptions
    {
        public string ApiKey { get; set; } = "";
    }
}
