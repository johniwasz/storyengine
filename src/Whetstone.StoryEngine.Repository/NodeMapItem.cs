using System.Collections.Generic;
using System.Diagnostics;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository
{
    [DebuggerDisplay("{Name}")]
    public class NodeMapItem
    {

        private List<NodeMapItem> _children = new List<NodeMapItem>();
        private List<NodeMapItem> _parents = new List<NodeMapItem>();
        private StoryNode _node;
        private Choice _choice;

        public NodeMapItem(StoryNode node)
        {
            _node = node;

        }

        public NodeMapItem(StoryNode node, Choice choice)
        {
            _node = node;
            _choice = choice;
        }


        public NodeMapItem AddChild(StoryNode node, Choice choice)
        {
            NodeMapItem mapItem = new NodeMapItem(node, choice);
            _children.Add(mapItem);

            return mapItem;
        }


        public Choice Choice
        {
            get { return _choice; }
        }

        public NodeMapItem AddParent(StoryNode node)
        {
            NodeMapItem mapItem = new NodeMapItem(node);
            _parents.Add(mapItem);

            return mapItem;
        }

        public List<NodeMapItem> Children
        {
            get { return _children; }
        }

        public List<NodeMapItem> Parent
        {
            get { return _parents; }
        }

        public string Name
        {

            get { return _node.Name; }
        }


    }
}
