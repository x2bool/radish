namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public interface IConnectionNodeBuilder
    {
        void HandleAction(ConnectionNodeAction action);
    }
}