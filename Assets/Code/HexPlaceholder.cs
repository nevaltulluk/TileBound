using UnityEngine;

namespace Code
{
    public class HexPlaceholder : MonoBehaviour
    {
        private Vector2Int hexCoordinates;

        public void Initialize(Vector2Int coordinates)
        {
            hexCoordinates = coordinates;
        }

        public void OnClick()
        {
            HexManager.Instance.PlaceHexFromPlaceholder(hexCoordinates, transform.position);
        }
    }
}