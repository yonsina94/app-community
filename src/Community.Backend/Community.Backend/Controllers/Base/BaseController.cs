using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Community.Backend.Services.Base;
using Community.Backend.Services.Constructor;
using Community.Backend.Services.Infraestructure;
using Community.Backend.Views.Base;
using Comunity.Models.Base;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace  Community.Backend.Controllers.Base{
    public abstract class BaseController<Tservice,Tview,Tmodel> where Tservice:IBaseService<Tmodel> where Tview:BaseView<Tmodel> where Tmodel:class,IBaseModel{
        private readonly IServiceConstructor Constructor;
        private readonly Tservice Service;

        protected BaseController(IServiceConstructor constructor){
            Constructor = constructor;
            Service = constructor.GetService<Tservice,Tmodel>();
        }

          /// <summary>
        /// Obtiene todas las entidades del modelo solicitante
        /// </summary>
        /// <returns>Lista de las vistas de los modelos resultantes</returns>
        [HttpGet]
        [Route("All", Name = "GetAll[controller]")]
        public virtual async Task<ActionResult<IList<Tview>>> GetAllAsync() => (await Service.GetAllAsync()).ConvertAll(m => (Tview)Activator.CreateInstance(typeof(Tview), m));

        /// <summary>
        /// Obtiene los detalles de una entidad de modelo en base a su identificador en base de datos
        /// </summary>
        /// <param name="id">Identificador unico de la entidad a consultar</param>
        /// <returns>Vista de la entidad de modelo resultante</returns>
        [HttpGet]
        [Route("Id", Name = "Get[controller]ByID")]
        public virtual async Task<ActionResult<Tview>> GetByIDAsync(Guid id) => (Tview)Activator.CreateInstance(typeof(Tview), await Service.GetByIDAsync(id));

        /// <summary>
        /// Actualiza una entidad de modelo o en su defecto crea el mismo en el sistema
        /// </summary>
        /// <param name="view">Vista de la entidad de modelo a evaluar</param>
        /// <returns>Resultado de la operacion de ingreso/actualizacion de datos</returns>
        [HttpPost]
        [Route("save", Name = "PostSave[controller]")]
        public async virtual Task<ActionResult<Result>> PostSaveChangesAsync([FromBody] JObject view)
        {
            Result Result = new Result().AddErrorMessage($"La vista recibida no es de tipo '{typeof(Tview).Name}' ");
            if (view.ToObject<Tview>() is Tview cview)
            {
                if (cview.ID == Guid.Empty)
                {
                   Result = await Service.CreateAsync(cview.ToModel());
                }
            }
            return Result;
        }

        [HttpPut]
        [Route("update",Name ="PutUpdate[controller]")]
        public virtual async Task<ActionResult<Result>> PutUpdateChangesAsync([FromBody]JObject view)
        {
            Result Result = new Result().AddErrorMessage($"La vista recibida no es de tipo '{typeof(Tview).Name}' ");
            if (view.ToObject<Tview>() is Tview cview)
            {
                if (cview.ID != Guid.Empty)
                {
                    Result = await Service.UpdateAsync(cview.ToModel());
                }
            }
            return Result;
        }

        /// <summary>
        /// Eliminar los detalles de una entidad de modelo en el sistema
        /// </summary>
        /// <param name="ID">Identificador unico del modelo a eliminar</param>
        /// <returns>Resultado de la operacion de eliminacion de datos</returns>
        [HttpDelete]
        [Route("remove", Name = "Delete[controller]")]
        public async virtual Task<ActionResult<Result>> DeleteRemoveChangesAsync([FromRoute]Guid ID) => await Service.DeleteAsync(ID);
    }
} 