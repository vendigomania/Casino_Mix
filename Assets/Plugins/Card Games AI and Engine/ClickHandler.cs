using UnityEngine;

namespace CGAIE
{

    /// <summary>
    /// Handles clicks on the cards for the DemoScript.
    /// </summary>
    public class ClickHandler : MonoBehaviour
    {
        /// <summary>
        /// index of the clicked card
        /// </summary>
        public int index;

        /// <summary>
        /// Called by the attached component. Notifies the demobase of the click.
        /// </summary>
        private void OnMouseDown()
        {
            DemoBase.NotifyClick(index);
        }

    }

}