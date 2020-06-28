using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebService.Services;
using WebService.Models;
using WebService.Models_HRA;
using WebService.Helpers;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApoloHRAController : ControllerBase
    {
        IConfiguration _configuration;
        private LogicToken logicToken;
        private readonly EsmeraldaContext _db;
       
        public ApoloHRAController(IConfiguration configuration,EsmeraldaContext db)
        {
            _configuration = configuration;
            logicToken = new LogicToken(_configuration);
            _db = db;

        }
        [HttpGet]
        [Route("inicio")]
        public string Get()
        {
            return "Api Funcionando Correctamente...";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// Test:"OK"
        [HttpGet]
        [Route("echoping")]
        public IActionResult EchoPing()
        {
            return Ok(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// Test:"OK"
        [HttpPost]
        [Route("token")]
        public IActionResult Login([FromBody] Usuario model)
        {
            try
            {
                ActionResult response = Unauthorized();
                Usuario usuario = model;
                var _user = logicToken.AuthenticateUsuario(usuario);
                if (_user != null)
                {
                    var token = logicToken.GenerateToken(usuario);
                    response = Ok(new { token = token });
                    return response;
                }
                //response = context.AddSospecha();
                return BadRequest("Revise sus credenciales...."); ;
            }
            catch (Exception)
            {
                return BadRequest("Error.... Intente más tarde.");
            }
            
        }
        /// <summary>
        /// Se Obtiene los datos del usuario logeado.
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        /// Test:"OK"
        [HttpPost]
        [Authorize]
        [Route("user")]
        public IActionResult getUser([FromBody] users users)
        {
            try
            {
                users cred = _db.users.Where(a => a.run == users.run).FirstOrDefault();
                return Ok(cred);
            }
            catch (Exception e)
            {
                return BadRequest("Error.... Intente más tarde." + e);
            }
           
        }

        /// <summary>
        /// Metodo de recuperar la ID de bd EME
        /// </summary>
        /// <param name="patients"></param>
        /// <returns>patient_id</returns>
        /// Test:"OK"
        [HttpPost]
        [Authorize]
        [Route("getPatient_ID")]
        public IActionResult getPatient_ID([FromBody] PacienteHRA pa)
        {
            try
            {
                Patients p = new Patients();
                if (String.IsNullOrEmpty(pa.run))
                {
                    p = _db.patients.Where(a => a.other_identification.Equals(pa.other_Id)).FirstOrDefault();
                }
                else
                {
                    p = _db.patients.Where(a => a.run.Equals(int.Parse(pa.run))).FirstOrDefault();
                }

                if (p != null) { return Ok(p.id); }
                else { return Ok(null); }
            }
            catch (Exception e)
            {
                return BadRequest("Error.... Intente más tarde." + e);
            }
            
        }
        /// <summary>
        /// Metodo para agregar un paciente.
        /// </summary>
        /// <param name="patients"></param>
        /// <returns></returns>
        /// Test ="OK"
        [HttpPost]
        [Authorize]
        [Route("AddPatients")]
        public IActionResult addPatients([FromBody] Patients patients)
        {
            try
            {
                //Revisar Otra vez si esta el paciente..
                _db.patients.Add(patients);
                var p =_db.SaveChanges();
                //Recuperar el dato ID
                long id = patients.id;
                return Ok(id);
            }
            catch (Exception e)
            {
                return BadRequest("Error.... Intente más tarde." + " Error:"+ e);
            }
            
        }
        /// <summary>
        /// Método de obtener getComuna 
        /// </summary>
        /// <param name="code_ids"></param>
        /// <returns>Obj Comuna con atributo id y name</returns>
        /// Teste = "OK"
        [HttpPost]
        [Authorize]
        [Route("getComuna")]
        public IActionResult getComuna([FromBody] string code_ids)
        {
            try
            {
                Communes communes = new Communes();
                var c = _db.communes.Where(c => c.code_deis.Equals(code_ids)).Select(per => new { per.id, per.name, per.code_deis });
                return Ok(c);
            }
            catch (Exception)
            {
                return BadRequest("Error.... Intente más tarde.");
            }
            
        }
        /// <summary>
        /// Metodo de Agregar una demografia
        /// </summary>
        /// <param name="demographics"></param>
        /// <returns></returns>
        /// Test = OK
        [HttpPost]
        [Authorize]
        [Route("AddDemograph")]
        public IActionResult addDemograph([FromBody] demographics demographics)
        {
            try
            {
                _db.Demographics.Add(demographics);
                _db.SaveChanges();
                return Ok("Se Guardo Correctamente la Demografía");
            }
            catch (Exception e)
            {
                return BadRequest("Error.....Intente más Tarde"+ e ) ;
            }        
        }

        /// <summary>
        /// Método donde se agrega .
        /// </summary>
        /// <param name="sospecha">json de la sospecha</param>
        /// <returns></returns>
        /// TESTE = OK 
        [HttpPost]
        [Authorize]
        [Route("addSospecha")]
        public IActionResult addSospecha([FromBody] Sospecha sospecha)
        {
            try
            {
                _db.suspect_cases.Add(sospecha);
                _db.SaveChanges();
                return Ok(sospecha.id);

            }
            catch (Exception e)
            {

                return BadRequest("No se guardo correctamente...." + e);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sospecha">
        /// ID
        /// reception_at
        /// receptor_id
        /// laboratory_id
        /// </param>
        /// <returns></returns>
        /// Test = OK
        [HttpPost]
        [Authorize]
        [Route("recepcionMuestra")]
        public IActionResult udpateSospecha([FromBody] Sospecha sospecha)
        {
            try
            {
                //Buscamos si exite la sospecha 
                Sospecha _sospecha = _db.suspect_cases.Find(sospecha.id);
                //Sospecha _sospecha = _db.suspect_cases.Where(c => c.id.Equals(sospecha.id)).SingleOrDefault();

                if (_sospecha!=null)
                {
                    //Actualizamos
                    _sospecha.reception_at = sospecha.reception_at;
                    _sospecha.receptor_id = sospecha.receptor_id;
                    _sospecha.laboratory_id = sospecha.laboratory_id;
                    _sospecha.updated_at = sospecha.updated_at;
                    _db.suspect_cases.Update(_sospecha);
                    _db.SaveChanges();
                    return Ok("Se Guardo correctamente...");
                }

                return BadRequest("No se guardo correctamente....");
            }
            catch (Exception e)
            {

                return BadRequest("No se guardo correctamente....");
            }
        }

        /// <summary>
        /// Update 
        /// </summary>
        /// <param name="sospecha">
        ///     pscr_sars_cov_2_at
        ///     pscr_sars_cov_2
        ///     validator_id
        ///     updated_at
        /// </param>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Authorize]
        [Route("resultado")]
        public IActionResult udpateResultado([FromBody] Sospecha sospecha)
        {
            try
            {
                //Buscamos si exite la sospecha 
                var _sospecha = _db.suspect_cases.Find(sospecha.id);
                if (_sospecha!=null)
                {
                    _sospecha.pscr_sars_cov_2_at = sospecha.pscr_sars_cov_2_at;
                    _sospecha.pscr_sars_cov_2 = sospecha.pscr_sars_cov_2;
                    _sospecha.validator_id = sospecha.validator_id;
                    _sospecha.updated_at = sospecha.updated_at;
                }
                
               //Revisar como funciona.. el update..
                _db.SaveChanges();
                return Ok("Exito... se actualizo los resultado..");
            }
            catch (Exception)
            {
                return BadRequest("No se guardo correctamente....");
            }
        }
        /// <summary>
        /// Obtener el paciente con una ID 
        /// </summary>
        /// <param name="sospecha">
        /// </param>
        /// <returns>Paciente</returns>
        /// TEST = OK
        [HttpGet]
        [Authorize]
        [Route("getPatients")]
        public IActionResult getPatients([FromBody] string buscador) 
        {
            try
            {
                int run = Convert.ToInt32(buscador);
                //Si quieres el primero, usar  .singledefault
                var paciente = _db.patients.Where(c => c.run.Equals(run));
                if (paciente==null)
                {
                    var pacienteOther = _db.patients.Where(c => c.other_identification.Equals(buscador)).SingleOrDefault();
                    return Ok(pacienteOther);
                }
                else
                {
                    return Ok(paciente);
                }
               
            }
            catch (Exception e)
            {
                return BadRequest("No se Encontro Paciente.... problema" + e);
            }     
        }

        /// <summary>
        /// Obtener el sospechas por el rut o other del paciente 
        /// </summary>
        /// <param name="sospecha">
        /// </param>
        /// <returns>Paciente</returns>
        /// TEST = OK
        [HttpGet]
        [Authorize]
        [Route("getSospecha")]
        public IActionResult getSospeha([FromBody] string buscador)
        {
            try
            { 

                int run = Int32.Parse(buscador);
                var paciente = _db.patients.Where(c => c.run.Equals(run)).FirstOrDefault();
                if (paciente == null)
                {
                    var pacienteOther = _db.patients.Where(c => c.other_identification.Equals(buscador)).FirstOrDefault();
                    var obj  = _db.suspect_cases.Where(c => c.patient_id.Equals(pacienteOther.id));
                    return Ok(obj);
                }
                else
                {
                    var obj1 = _db.suspect_cases.Where(c => c.patient_id.Equals(paciente.id));
                    return Ok(obj1);
                }
            }
            catch (Exception e)
            {
                return BadRequest("No se Encontro sospecha.... problema" + e);
            }
        }

        /// <summary>
        /// Obtener el Demograph por el rut o other del paciente 
        /// </summary>
        /// <param name="sospecha">
        /// </param>
        /// <returns>Paciente</returns>
        /// TEST = OK
        [HttpGet]
        [Authorize]
        [Route("getDemograph")]
        public IActionResult getDemograph([FromBody] string buscador)
        {
            try
            {   

                int run = Int32.Parse(buscador);
                var paciente = _db.patients.Where(c => c.run.Equals(run)).FirstOrDefault();
                if (paciente == null)
                {
                    var pacienteOther = _db.patients.Where(c => c.other_identification.Equals(buscador)).FirstOrDefault();
                    var obj = _db.Demographics.Where(c => c.patient_id.Equals(pacienteOther.id));
                    return Ok(obj);
                }
                else
                {
                    var obj1 = _db.Demographics.Where(c => c.patient_id.Equals(paciente.id));
                    return Ok(obj1);
                }
            }
            catch (Exception e)
            {
                return BadRequest("No se Encontro sospecha.... problema" + e);
            }
        }
    }
}
