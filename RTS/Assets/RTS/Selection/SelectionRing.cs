namespace RTS.Selection
{
    public class SelectionRing : SelectionListener
    {
        // private SelectionRing _selectionRing = null;

        protected override void OnSelect()
        {
            // UnitMediator.HandleCommandPointVisibility(true);
            // GetRing().SetSelectionVisibility(true);
        }

        protected override void OnDeselect()
        {
            // UnitMediator.HandleCommandPointVisibility(false);
            // GetRing().SetSelectionVisibility(false);
            // if (_selectionRing.Visible == false)
            //     ReturnRing();
        }

        protected override void OnHoverEnter()
        {
            // GetRing().SetHoverVisibility(true);
        }

        protected override void OnHoverExit()
        {
            // GetRing().SetHoverVisibility(false);
            // if(_selectionRing.Visible == false)
            // ReturnRing();
        }
        
        // private void ReturnRing()
        // {
        //     if (_selectionRing != null)
        //     {
        //         _selectionRing.Clear();
        //         _selectionRing.gameObject.SetActive(false);
        //         // SelectionRingPool.Instance.Release(_selectionRing);
        //         _selectionRing = null;
        //     }
        // }
    }
}