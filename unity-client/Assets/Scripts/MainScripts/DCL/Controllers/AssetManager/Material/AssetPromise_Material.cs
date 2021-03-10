using System;

namespace DCL
{
    public class AssetPromise_Material : AssetPromise<Asset_Material>
    {
        protected override void OnAfterLoadOrReuse()
        {
            throw new NotImplementedException();
        }

        protected override void OnBeforeLoadOrReuse()
        {
            throw new NotImplementedException();
        }

        protected override void OnCancelLoading()
        {
            throw new NotImplementedException();
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            throw new NotImplementedException();
        }

        public override object GetId()
        {
            throw new NotImplementedException();
        }
    }
}