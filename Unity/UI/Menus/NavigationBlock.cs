using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using OpenGET;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Specialised UI navigation block. Use this to control navigation between UI elements and other blocks within a particular area.
    /// Adheres to (and builds upon) Unity's built-in UI navigation system.
    /// Note: This does NOT do any selection state management itself, that is left to Unity or other OpenGET modules. 
    /// </summary>
    public class NavigationBlock : AutoBehaviour
    {
        /// <summary>
        /// Provides access to a single Unity UI Navigation object.
        /// </summary>
        public interface IElement
        {
            public Navigation navigation { get; set; }
            public Selectable selectable { get; }
        };

        /// <summary>
        /// Wrapper for handling both custom & built-in UI.
        /// </summary>
        [System.Serializable]
        public sealed class Element : IElement
        {
            public readonly IElement main;

            [SerializeField]
            [Tooltip("UI element that can be navigated to/from.")]
            private Selectable navigable;

            /// <summary>
            /// Check if this is "automatically" set.
            /// </summary>
            public bool isAutomatic => navigable == null || main != null;

            public Navigation navigation
            {
                get { return main != null ? main.navigation : navigable.navigation; }
                set {
                    if (main != null)
                    {
                        main.navigation = value;
                    }
                    else
                    {
                        navigable.navigation = value;
                    }
                }
            }

            public Selectable selectable => main != null ? main.selectable : navigable;

            public Element(Selectable element)
            {
                navigable = element;
            }

            public Element(IElement element)
            {
                main = element;
            }

            public Element() { }

        }

        [System.Serializable]
        public struct Neighbours
        {
            public NavigationBlock up;
            public NavigationBlock down;
            public NavigationBlock left;
            public NavigationBlock right;
        }

        public enum LayoutDirection
        {
            Vertical = 0,
            Horizontal
        }

        /// <summary>
        /// Layout direction of the children; must be either vertical or horizontal, cannot be both.
        /// </summary>
        public LayoutDirection layoutDirection = LayoutDirection.Vertical;

        /// <summary>
        /// Neighbouring blocks, if any.
        /// </summary>
        public Neighbours neighbours;

        [SerializeField]
        [Tooltip("Navigable elements within this block. These may be setup dynamically at runtime.")]
        private List<Element> children = new();

        [Tooltip("When enabled, automatically finds & hooks up children.")]
        public bool automatic = true;

        /// <summary>
        /// Multiple "isDirty" flags due to different aspects needing updates at different times.
        /// </summary>
        [System.Flags]
        public enum Refresh
        {
            None = 0,
            Init = 1,
            Navigation = 2
        }

        /// <summary>
        /// Do the children need their navigation updated?
        /// </summary>
        private Refresh refresh = Refresh.Init | Refresh.Navigation;

        /// <summary>
        /// Set refresh flags to trigger an update.
        /// </summary>
        public void SetDirty(Refresh flags)
        {
            refresh |= flags;
        }

        /// <summary>
        /// Get a list of the whole "neighbourhood" in relation to this block.
        /// </summary>
        public List<NavigationBlock> GetNeighbourhood(HashSet<NavigationBlock> neighbourhood = null)
        {
            List<NavigationBlock> neighbourList = new() { this };
            if (neighbourhood == null)
            {
                neighbourhood = new HashSet<NavigationBlock>() { this };
            }
            neighbourhood.Add(this);

            // Step through neighbours, prioritising those in the same layout direction towards the right and down
            PriorityQueue<NavigationBlock> candidates = new();
            if (neighbours.up != null && !neighbourhood.Contains(neighbours.up))
            {
                neighbourList.AddRange(neighbours.up.GetNeighbourhood(neighbourhood));
            }
            if (neighbours.down != null && !neighbourhood.Contains(neighbours.down))
            {
                neighbourList.AddRange(neighbours.down.GetNeighbourhood(neighbourhood));
            }
            if (neighbours.left != null && !neighbourhood.Contains(neighbours.left))
            {
                neighbourList.AddRange(neighbours.left.GetNeighbourhood(neighbourhood));
            }
            if (neighbours.right != null && !neighbourhood.Contains(neighbours.right))
            {
                neighbourList.AddRange(neighbours.right.GetNeighbourhood(neighbourhood));
            }

            return neighbourList;
        }

        /// <summary>
        /// Find an element among selectable children of this NavigationBlock.
        /// </summary>
        public Element FindChild(GameObject obj)
        {
            for (int i = 0, counti = children.Count; i < counti; i++)
            {
                if (children[i].selectable != null && children[i].selectable.gameObject == obj)
                {
                    return children[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Check if a GameObject is one of the selectable children of this NavigationBlock.
        /// </summary>
        public bool HasChild(GameObject obj)
        {
            return FindChild(obj) != null;
        }

        /// <summary>
        /// Check if a GameObject is a selectable child in the neighbourhood of this NavigationBlock.
        /// </summary>
        public bool HasChildInNeighbourhood(GameObject obj, HashSet<NavigationBlock> explored = null)
        {
            if (explored.Contains(this))
            {
                // Early out, already explored
                return false;
            }

            // Add to explored list
            if (explored == null)
            {
                explored = new() { this };
            }
            else
            {
                explored.Add(this);
            }

            // Check self
            bool found = HasChild(obj);
            if (found)
            {
                return found;
            }

            // Search neighbours
            if (neighbours.up != null)
            {
                found = neighbours.up.HasChildInNeighbourhood(obj, explored);
            }
            if (neighbours.down != null && !found)
            {
                found = neighbours.down.HasChildInNeighbourhood(obj, explored);
            }
            if (neighbours.left != null && !found)
            {
                found = neighbours.left.HasChildInNeighbourhood(obj, explored);
            }
            if (neighbours.right != null && !found)
            {
                found = neighbours.right.HasChildInNeighbourhood(obj, explored);
            }

            return found;
        }

        /// <summary>
        /// Attempts to return the closest active child element. Returns null if invalid or there are no active children.
        /// </summary>
        public Element GetClosestElement(Element other)
        {
            if (other.selectable == null)
            {
                return null;
            }

            Element best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0, counti = children.Count; i < counti; i++)
            {
                Selectable child = children[i].selectable;
                if (child != null && child.isActiveAndEnabled)
                {
                    float dist = Vector2.SqrMagnitude(other.selectable.transform.position - child.transform.position);
                    if (dist < bestDistance)
                    {
                        best = children[i];
                        bestDistance = dist;
                    }
                }
            }
            return best;
        }

        protected void OnTransformChildrenChanged()
        {
            refresh |= Refresh.Init;
        }

        /// <summary>
        /// Insert a new child without triggering a refresh.
        /// </summary>
        public Element AddElement(IElement element)
        {
            return AddElement(new Element(element));
        }

        /// <summary>
        /// Insert a new child without triggering a refresh.
        /// </summary>
        public Element AddElement(Selectable element)
        {
            return AddElement(new Element(element));
        }

        /// <summary>
        /// Insert a new child without triggering a refresh.
        /// </summary>
        public Element AddElement(Element element)
        {
            bool vert = layoutDirection == LayoutDirection.Vertical;
            bool doInsert = false;
            for (int i = 0, counti = children.Count; i < counti; i++)
            {
                // Check if this is a suitable place to insert the element. Ignores inactive elements.
                Element child = children[i];
                doInsert = child != null && child.selectable != null && child.selectable.isActiveAndEnabled && (vert ?
                    (-element.selectable.transform.position.y < -child.selectable.transform.position.y) :
                    (element.selectable.transform.position.x < child.selectable.transform.position.x));

                if (doInsert)
                {
                    children.Insert(i, element);
                    break;
                }
            }

            if (!doInsert)
            {
                children.Add(element);
            }
            return element;
        }
        
        /// <summary>
        /// Always refresh in the update after OnEnable.
        /// </summary>
        protected void OnEnable()
        {
            Init();
            UpdateNavigation();

            // Refresh on updates for good measure
            refresh = Refresh.Navigation | Refresh.Init;
        }

        /// <summary>
        /// Automatic navigation update handler for dynamic content.
        /// </summary>
        private void Init()
        {
            if (automatic)
            {
                // First, remove all automatic elements
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    if (children[i] == null || children[i].selectable == null || children[i].isAutomatic)
                    {
                        children.SwapRemoveAt(i);
                    }
                }

                // Try and get selectables & IElement objects
                for (int i = 0, counti = transform.childCount; i < counti; i++)
                {
                    Transform child = transform.GetChild(i);

                    IElement element = child.GetComponent<IElement>();
                    if (element != null && element.selectable != null && children.FirstOrDefault(x => x != null && (x.main == element || (x.selectable != null && child.GetComponent<Selectable>() == x.selectable))) == null)
                    {
                        AddElement(element);
                    }
                    else
                    {
                        Selectable selectable = child.GetComponent<Selectable>();
                        if (selectable != null && children.FirstOrDefault(x => x.selectable == selectable) == null)
                        {
                            AddElement(selectable);
                        }
                    }
                }
            }

            // Order top-to-bottom or left-to-right depending on layout direction
            bool vert = layoutDirection == LayoutDirection.Vertical;
            children = children.OrderBy(
                element => vert ? -element.selectable.transform.position.y : element.selectable.transform.position.x
            ).ToList();

            refresh &= ~Refresh.Init;
        }

        /// <summary>
        /// Setup navigation for children of this block.
        /// </summary>
        private void UpdateNavigation()
        {
            // Now step through and setup navigation
            Selectable prev = null;
            int nextIndex = 0;
            Selectable next = null;
            bool vert = layoutDirection == LayoutDirection.Vertical;
            for (int i = 0, counti = children.Count; i < counti; i++)
            {
                // Get the next active element
                if (i < counti - 1 && nextIndex <= i)
                {
                    nextIndex = children.FindIndex(nextIndex + 1, x => x.selectable != null && x.selectable.isActiveAndEnabled);
                    next = nextIndex >= 0 && i != nextIndex ? children[nextIndex].selectable : null;
                }
                else
                {
                    next = null;
                }

                Element child = children[i];
                if (child.selectable != null && child.selectable.isActiveAndEnabled)
                {
                    // Setup explicit navigation
                    child.navigation = new Navigation()
                    {
                        mode = Navigation.Mode.Explicit,
                        wrapAround = false,
                        selectOnUp = vert && prev != null ? prev : neighbours.up?.GetClosestElement(child)?.selectable,
                        selectOnDown = vert && next != null ? next : neighbours.down?.GetClosestElement(child)?.selectable,
                        selectOnLeft = !vert && prev != null ? prev : neighbours.left?.GetClosestElement(child)?.selectable,
                        selectOnRight = !vert && next != null ? next : neighbours.right?.GetClosestElement(child)?.selectable,
                    };
                    prev = child.selectable;
                }

                if (nextIndex >= 0 && nextIndex > i + 1)
                {
                    i = nextIndex - 1;
                }
            }

            refresh &= ~Refresh.Navigation;
        }

        private void Update()
        {
            if ((refresh & Refresh.Init) != 0)
            {
                Init();
            }
        }

        private void LateUpdate()
        {
            // Only refresh nav AFTER init has completed
            if ((refresh & Refresh.Navigation) != 0 && (refresh & Refresh.Init) == 0)
            {
                UpdateNavigation();
            }
        }

    }

}
