using UnityEngine;

namespace Code
{
    public class HexPlaceholder : MonoBehaviour
    {
        private Vector2Int hexCoordinates;
        HexManager hexManager;

        public void Initialize(Vector2Int coordinates)
        {
            hexManager = MainContainer.Instance.Resolve<HexManager>();
            hexCoordinates = coordinates;
        }

        public void OnClick()
        {
            hexManager.PlaceHexFromPlaceholder(hexCoordinates, transform.position);
        }
    }
}