namespace Image
{
    public enum NodeDataType
    {
        Drive,
        Folder, 
        File
    }

    public class TreeNode
    {
        public NodeDataType Type { get; set; }
        public string Path { get; set; }
    }
}
