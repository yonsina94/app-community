using System;
using Comunity.Models.Base;

namespace Community.Backend.Views.Base{
    public abstract class BaseView<Tmodel> where Tmodel:class,IBaseModel{
        public Guid ID{get;set;}

        public BaseView(){

        }

        public BaseView(Tmodel model){
            ID = model.ID;
        }

        public abstract Tmodel ParseModel();

        public Tmodel ToModel(){
            var model = ParseModel();
            model.ID = ID;
            return model;
        }
    }
}