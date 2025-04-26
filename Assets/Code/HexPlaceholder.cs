using UnityEngine;

namespace Code
{
    public class HexPlaceholder : MonoBehaviour
    {
        private Vector2Int _hexCoordinates;
        HexManager _hexManager;

        public void Initialize(Vector2Int coordinates)
        {
            _hexManager = MainContainer.instance.Resolve<HexManager>();
            _hexCoordinates = coordinates;
        }

        public void OnClick()
        {
            _hexManager.PlaceHexFromPlaceholder(_hexCoordinates, transform.position);
        }
    }
}