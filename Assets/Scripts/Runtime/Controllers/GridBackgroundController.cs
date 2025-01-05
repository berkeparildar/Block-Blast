using Runtime.Data.ValueObjects;
using UnityEngine;

namespace Runtime.Controllers
{
    public class GridBackgroundController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer borderSpriteRenderer;
        [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
        [SerializeField] private GameObject topPanel;
        private GridData _data;

        public void SetData(GridData gridData)
        {
            _data = gridData;
        }

        public void SetGridBackground()
        {
            backgroundSpriteRenderer.size = new Vector2(_data.GridColumnSize + 0.25f, _data.GridRowSize + 0.25f);
            borderSpriteRenderer.size = new Vector2(_data.GridColumnSize + 0.25f, _data.GridRowSize + 0.25f);
            transform.localPosition = new Vector2((_data.GridColumnSize - 1) / 2f, (_data.GridRowSize - 1) / 2f);
            topPanel.transform.localPosition = new Vector2(0, (_data.GridRowSize / 2f) + 5.125f);
        }
    }
}