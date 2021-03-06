using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebService.Models;
using WebService.Models_HRA;
using WebService.Request;
using WebService.Services;

namespace WebService.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ApoloHRAController : ControllerBase
    {
        private readonly ILogger<ApoloHRAController> _logger;
        private readonly EsmeraldaContext _db;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="db"></param>
        public ApoloHRAController(ILogger<ApoloHRAController> logger, EsmeraldaContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Usado para verificar que el servicio responde al llamado.
        /// </summary>
        /// <returns>Devuelve un valor verdadero si el servicio responde</returns>
        [HttpGet]
        [Route("echoping")]
        public ActionResult<bool> EchoPing()
        {
            return Ok(true);
        }

        /// <summary>
        /// Recupera un usuario existente en el monitor 
        /// </summary>
        /// <remarks>
        /// Recupera un usuario del monitor dado el RUN
        /// 
        /// Solicitud de ejemplo:
        /// 
        ///     POST /apolohra/user
        ///     {
        ///       "run": 12345678
        ///     }
        /// 
        /// </remarks>
        /// <param name="users"></param>
        /// <returns></returns>
        /// <response code="200">Devuelve la información del usuario</response>
        /// <response code="400">Mensaje descriptivo del error</response>
        [HttpPost]
        [Authorize]
        [Route("user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(users))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetUser([FromBody] users users)
        {
            try
            {
                var cred = _db.users.FirstOrDefault(a => a.run == users.run);
                return Ok(cred);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Usuario no encontrado user:{users}", users);
                return BadRequest("Error.... Intente más tarde." + e);
            }
        }

        /// <summary>
        /// Recupera el identificador interno de un paciente en el monitor esmeralda.
        /// </summary>
        /// <remarks>
        /// La búsqueda se realiza por el run del paciente o por otro identificador.
        ///
        /// Solicitudes de ejemplo:
        ///
        ///     POST /apolohra/getpatient_id
        ///     {
        ///         "run": "12838526"
        ///     }
        /// 
        /// </remarks>
        /// <param name="pa">Estructura con el identificador del paciente</param>
        /// <returns>El identficador interno, en caso de no encontrar devuelve un nulo</returns>
        /// <response code="200">Identificador interno del paciente en el monitor</response>
        /// <response code="400">Mensaje descriptivo del error</response>
        [HttpPost]
        [Authorize]
        [Route("getPatient_ID")]
        [ProducesResponseType(typeof(int?),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetPatientId([FromBody] PacienteHRA pa)
        {
            try
            {
                Patients p;
                if (string.IsNullOrEmpty(pa.run))
                    p = _db.patients.FirstOrDefault(a => a.other_identification.Equals(pa.other_Id));
                else
                    p = _db.patients.FirstOrDefault(a => a.run.Equals(int.Parse(pa.run)));

                return p != null? Ok(p.id): Ok(null);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Paciente no recuperado, paciente:{pa}", pa);
                return BadRequest("Error.... Intente más tarde." + e);
            }
        }

        /// <summary>
        /// Agrega paciente inexistente en el monitor esmeralda
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        ///     POST /apolohra/addpatients
        ///     {
        ///         "run": 11111111,
        ///         "dv": "1",
        ///         "name": "Javier Andrés",
        ///         "fathers_family": "Mandiola",
        ///         "mothers_family": "Ovalle",
        ///         "gender": "male",
        ///         "birthday": "1975-04-03",
        ///         "status": "",
        ///         "created_at": "2020-10-28T12:00:00"
        ///         "updated_at": "2020-10-28T12:00:00"
        ///     }
        ///  
        /// </remarks>
        /// <param name="patients">Datos del paciente que serán ingresados</param>
        /// <returns>
        /// Identificador interno del paciente creado
        /// </returns>
        /// <response code="200">El identificador interno del paciente creado</response>
        /// <response code="400">Mensaje detallado del error</response>
        [HttpPost]
        [Authorize]
        [Route("AddPatients")]
        [ProducesResponseType(typeof(int?),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string),StatusCodes.Status400BadRequest)]
        public IActionResult AddPatients([FromBody] Patients patients)
        {
            try
            {
                _db.patients.Add(patients);
                _db.SaveChanges();

                return Ok(patients.id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Paciente no guasrdado, paciente:{patients}", patients);
                return BadRequest("Error.... Intente más tarde." + " Error:"+ e);
            }
        }

        /// <summary>
        /// Recupera la comuna indicando el código DEIS del MINSAL.
        /// </summary>
        /// <remarks>
        /// Ejemplo solicitud:
        ///
        ///     POST /apolohra/getcomuna
        ///     "2101"
        /// 
        /// </remarks>
        /// <param name="codeIds">Código DEIS del establecimiento.</param>
        /// <returns>Devuelve la comuna</returns>
        /// <response code="200">Devuelve la comuna asociada al código DEIS</response>
        /// <response code="400">Mensaje detallado del error</response>
        /// <response code="401">No está autenticado</response>
        [HttpPost]
        [Authorize]
        [Route("getComuna")]
        [ProducesResponseType(typeof(Communes),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string),StatusCodes.Status400BadRequest)]
        public ActionResult<Communes> GetComuna([FromBody] string codeIds)
        {
            try
            {
                var c = _db.communes
                           .FirstOrDefault(x => x.code_deis.Equals(codeIds));
                return Ok(c);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Comuna con error, comuna id:{code_ids}", codeIds);
                return BadRequest("Error.... Intente más tarde.");
            }
        }

        /// <summary>
        /// Agrega datos demográficos al paciente.
        /// </summary>
        /// <remarks>
        /// Agrega información de la residencia y contacto del paciente
        ///
        /// Ejemplo solicitud:
        ///
        ///     POST /agregarhra/adddemograph
        ///     {
        ///         "street_type": "Calle"
        ///         "address": "Avelino Contardo",
        ///         "number": "1092",
        ///         "department": "104",
        ///         "nationality": "Chile",
        ///         "commune_id": 12,
        ///         "region_id": 2,
        ///         "latitude": -23.62272150,
        ///         "longitude": -70.38984400,
        ///         "telephone": "552244405",
        ///         "email": "test@mail.cl",
        ///         "patient_id": 1,
        ///         "created_at": "2020-11-28T12:00:00",
        ///         "updated_at": "2020-11-28T12:00:00"
        ///     }
        /// </remarks>
        /// <param name="demographics">Datos demográficos del paciente</param>
        /// <returns>Un mensaje de existo de la operacion</returns>
        /// <response code="200">Un mensaje del éxito de la operación</response>
        /// <response code="400">Mensaje detallado del error</response>
        /// <response code="401">No está autenticado</response>
        [HttpPost]
        [Authorize]
        [Route("AddDemograph")]
        [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string),StatusCodes.Status400BadRequest)]
        public IActionResult AddDemograph([FromBody] demographics demographics)
        {
            try
            {
                _db.demographics.Add(demographics);
                _db.SaveChanges();
                return Ok("Se Guardo Correctamente la Demografía");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Demografico no agregado, demographics:{demographics}", demographics);
                return BadRequest("Error.....Intente más Tarde"+ e ) ;
            }        
        }

        /// <summary>
        /// Agrega una nueva sospecha de COVID al monitor esmeralda
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        ///     POST /agregarhra/addsospecha
        ///     {
        ///         "gender": "male",
        ///         "age": 45,
        ///         "sample_at": "2020-10-27T08:30:00",
        ///         "epidemiological_week": 7,
        ///         "run_medic": "22222222",
        ///         "symptoms": "Si",
        ///         "symptoms_at": "2020-10-25T00:00:00",
        ///         "pscr_sars_cov_2": "pending",
        ///         "sample_type": "TÓRULAS NASOFARÍNGEAS",
        ///         "epivigila": 1024,
        ///         "gestation": false,
        ///         "gestation_week": null,
        ///         "close_contact": true,
        ///         "functionary": true,
        ///         "patient_id": 1,
        ///         "laboratory_id": 3,
        ///         "establishment_id": 3799,
        ///         "user_id": 1,
        ///         "created_at": "2020-10-28T09:00:00",
        ///         "updated_at": "2020-10-28T09:00:00"
        ///     }
        /// </remarks>
        /// <param name="sospecha">Información que es necesaria para la creación de la sospecha</param>
        /// <returns>Devuelve el número del caso de sospecha</returns>
        /// <response code="200">El número del caso de sospecha en el monitor</response>
        /// <response code="400">Mensaje detallado del error</response>
        /// <response code="401">No autenticado</response>
        [HttpPost]
        [Authorize]
        [Route("addSospecha")]
        [ProducesResponseType(typeof(int?),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string),StatusCodes.Status400BadRequest)]
        public IActionResult AddSospecha([FromBody] Sospecha sospecha)
        {
            try
            {
                var suspectCase = new SuspectCase
                {
                    age = sospecha.age,
                    gender = sospecha.gender,
                    sample_at = sospecha.sample_at,
                    epidemiological_week = sospecha.epidemiological_week,
                    run_medic = sospecha.run_medic,
                    symptoms = sospecha.symptoms == "Si",
                    pcr_sars_cov_2 = sospecha.pscr_sars_cov_2,
                    sample_type = sospecha.sample_type,
                    epivigila = sospecha.epivigila,
                    gestation = sospecha.gestation,
                    gestation_week = sospecha.gestation_week,
                    close_contact = sospecha.close_contact,
                    functionary = sospecha.functionary,
                    patient_id = sospecha.patient_id,
                    establishment_id = sospecha.establishment_id,
                    user_id = sospecha.user_id,
                    created_at = sospecha.created_at,
                    updated_at = sospecha.updated_at
                };

                _db.suspect_cases.Add(suspectCase);
                _db.SaveChanges();
                return Ok(suspectCase.id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Sospecha no agregada, sospecha:{sospecha}", sospecha);
                return BadRequest("No se guardo correctamente...." + e);
            }
        }

        /// <summary>
        /// Informa la recepción de la muestra por parte del laboratorio
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        ///     POST /apolohra/recepcionmuestra
        ///     {
        ///         "id": 1,
        ///         "reception_at": "2020-10-28T18:00:00",
        ///         "receptor_id": 1,
        ///         "laboratory_id": 3,
        ///         "updated_at": "2020-10-28T18:00"
        ///     }
        /// 
        /// </remarks>
        /// <param name="sospecha">Datos de la recepción de la muestra</param>
        /// <returns>Un mensaje del resultado de la operacion</returns>
        /// <response code="200">Un mensaje que la recepción se realizó</response>
        /// <response code="400">Mensaje detallado del error</response>
        /// <response code="401">No autenticado</response>
        [HttpPost]
        [Authorize]
        [Route("recepcionMuestra")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult UdpateSospecha([FromBody] Sospecha sospecha)
        {
            try
            {
                var sospechaActualizada = _db.suspect_cases.Find(sospecha.id);

                if (sospechaActualizada == null) return BadRequest("No se guardo correctamente....");

                sospechaActualizada.reception_at = sospecha.reception_at;
                sospechaActualizada.receptor_id = sospecha.receptor_id;
                sospechaActualizada.laboratory_id = sospecha.laboratory_id;
                sospechaActualizada.updated_at = sospecha.updated_at;

                _db.SaveChanges();

                return Ok("Se Guardo correctamente...");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Sospecha no actualizada, sospeche:{sospecha}", sospecha);
                return BadRequest("No se guardo correctamente....");
            }
        }

        /// <summary>
        /// Informa la entrega del resultado de la muestra al caso de sospecha
        /// </summary>
        /// <remarks>
        /// Ejemplo de la solicitud:
        ///
        ///     POST /apolohra/resultado
        ///     {
        ///         "id": 1,
        ///         "pscr_sars_cov_2_at": "2020-08-29T10:30:22",
        ///         "pscr_sars_cov_2": "negative",
        ///         "validator_id": 1,
        ///         "updated_at": "2020-08-29T10:30:22"
        ///     }
        ///
        /// </remarks>
        /// <param name="sospecha">Datos de la entrega del resultado</param>
        /// <returns></returns>
        /// <response code="200">Un mensaje que se registró el resultado de la muestra</response>
        /// <response code="400">Mensaje detallado del error</response>
        /// <response code="401">No autenticado</response>
        [HttpPost]
        [Authorize]
        [Route("resultado")]
        [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string),StatusCodes.Status400BadRequest)]
        public IActionResult UdpateResultado([FromBody] Sospecha sospecha)
        {
            try
            {
                var sospechaActualizada = _db.suspect_cases.Find(sospecha.id);

                if (sospechaActualizada == null) return NotFound(sospecha);

                sospechaActualizada.pcr_sars_cov_2_at = sospecha.pscr_sars_cov_2_at;
                sospechaActualizada.pcr_sars_cov_2 = sospecha.pscr_sars_cov_2;
                sospechaActualizada.validator_id = sospecha.validator_id;
                sospechaActualizada.updated_at = sospecha.updated_at;
                
                _db.SaveChanges();
                return Ok("Exito... se actualizo los resultado..");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Resultado no actualizado, sospecha:{sospecha}", sospecha);
                return BadRequest("No se guardo correctamente....");
            }
        }

        /// <summary>
        /// Recupera el paciente dado un run u otro identificador
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud
        ///
        ///     GET /apolohra/getpatients
        ///     "11111111"
        /// 
        /// </remarks>
        /// <param name="buscador">RUN u otro identificador</param>
        /// <returns>Paciente</returns>
        /// <response code="200">Información del paciente</response>
        /// <response code="400">Mensaje detallado del error</response>
        /// <response code="401">No autenticado</response>
        [HttpGet]
        [Authorize]
        [Route("getPatients")]
        [ProducesResponseType(typeof(Patients), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetPatients([FromBody] string buscador) 
        {
            try
            {
                var paciente = RecuperarPaciente(buscador);
                return Ok(paciente);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "No se puede recuperar paciente:{buscador}", buscador);
                return BadRequest("No se Encontro Paciente.... problema" + e);
            }     
        }

        /// <summary>
        /// Recupera todos los casos de sospechas de un paciente.
        /// </summary>
        /// <remarks>
        /// El parámetro de la solicitud debe ser el RUN sin digito verificador u otro
        /// identificador (Pasaporte,etc)
        /// Ejemplo de solicitud:
        ///
        ///     GET /apolohra/getsospecha
        ///     "11111111"
        /// 
        /// </remarks>
        /// <param name="buscador">RUN o DNI del paciente a consultar</param>
        /// <returns>Un listado con los casos de sospecha que el paciente tiene</returns>
        /// <response code="200">Un listado con los casos de sospechas asociados al paciente</response>
        /// <response code="400">Mensaje detallado del error</response>
        /// <response code="401">No autenticado</response>
        [HttpGet]
        [Authorize]
        [Route("getSospecha")]
        [ProducesResponseType(typeof(List<Sospecha>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string),StatusCodes.Status400BadRequest)]
        public IActionResult GetSospeha([FromBody] string buscador)
        {
            try
            { 
                var paciente = RecuperarPaciente(buscador);
                var sospecha = _db.suspect_cases.Where(c => c.patient_id.Equals(paciente.id))
                                  .Select(
                                       s => new Sospecha
                                       {
                                           id = s.id,
                                           age = s.age,
                                           gender = s.gender,
                                           sample_at = s.sample_at,
                                           epidemiological_week = s.epidemiological_week,
                                           run_medic = s.run_medic,
                                           symptoms = s.symptoms.HasValue?s.symptoms.Value? "Si": "No":"No",
                                           pscr_sars_cov_2 = s.pcr_sars_cov_2,
                                           pscr_sars_cov_2_at = s.pcr_sars_cov_2_at,
                                           sample_type = s.sample_type,
                                           epivigila = s.epivigila,
                                           gestation = s.gestation,
                                           gestation_week = s.gestation_week,
                                           close_contact = s.close_contact,
                                           functionary = s.functionary,
                                           patient_id = s.patient_id,
                                           establishment_id = s.establishment_id,
                                           user_id = s.user_id,
                                           created_at = s.created_at,
                                           updated_at = s.updated_at,
                                           symptoms_at = s.symptoms_at,
                                           observation = s.observation
                                       }
                                   ).ToList();
                return Ok(sospecha);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "No se pudo recuperar sospecha del paciente:{buscador}", buscador);
                return BadRequest("No se Encontro sospecha.... problema" + e);
            }
        }

        /// <summary>
        /// Recupera los datos demográficos del paciente
        /// </summary>
        /// <remarks>
        /// El parámetro de la solicitud debe ser el RUN sin digito verificador u otro
        /// identificador (Pasaporte,etc)
        /// Ejemplo de solicitud:
        ///
        ///     GET /apolohra/getdemograph
        ///     "11111111"
        /// 
        /// </remarks>
        /// <param name="buscador">RUN u otro identificador del paciente</param>
        /// <returns>Datos demográficos del paciente</returns>
        /// <response code="200">Datos demográficos del paciente</response>
        /// <response code="400">Mensaje detallado del error</response>
        /// <response code="401">No autenticado</response>
        [HttpGet]
        [Authorize]
        [Route("getDemograph")]
        [ProducesResponseType(typeof(demographics),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string),StatusCodes.Status400BadRequest)]
        public IActionResult GetDemograph([FromBody] string buscador)
        {
            try
            {   
                var paciente = RecuperarPaciente(buscador);
                var demographic = _db.demographics.FirstOrDefault(c => c.patient_id.Equals(paciente.id));
                return Ok(demographic);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "No se pudo recuperar demografico del paciente:{buscador}", buscador);
                return BadRequest("No se Encontro sospecha.... problema" + e);
            }
        }

        private Patients RecuperarPaciente(string buscador)
        {
            var run = int.Parse(buscador);
            var paciente = _db.patients.FirstOrDefault(c => c.run.Equals(run));
            if (paciente == null)
            {
                paciente = _db.patients.FirstOrDefault(c => c.other_identification.Equals(buscador));
            }

            return paciente;
        }
        /// <summary>
        /// Recupera el caso de sospecha con sus datos relacionados
        /// </summary>
        /// <remarks>
        /// Recupera el caso de sospecha dado el número del caso.
        /// Ejemplo de solicitud
        ///
        ///     POST /apolohra/getsuspectcase
        ///     1
        /// 
        /// </remarks>
        /// <param name="idCase">Número del caso</param>
        /// <response code="200">Datos del caso de sospecha</response>
        /// <response code="400">Mensaje detallado del error</response>
        /// <response code="401">No autenticado</response>
        [HttpPost]
        [Authorize]
        [Route("getSuspectCase")]
        [ProducesResponseType(typeof(CasoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetSuspectCase([FromBody] long idCase)
        {
            try
            {
                var caso = _db.suspect_cases.FirstOrDefault(x => x.id == idCase);
                if (caso == null)
                {
                    return BadRequest("No existe el caso");
                }
                var patient = _db.patients.FirstOrDefault(x => x.id == caso.patient_id);
                if (patient == null)
                {
                    return BadRequest("No existe el paciente");
                }
                var demographic = _db.demographics.FirstOrDefault(x => x.patient_id == patient.id);
                if (demographic == null)
                {
                    return BadRequest("No existe el demografico");
                }
                object retorno = new CasoResponse
                {
                    caso = new Sospecha
                    {
                        id = caso.id,
                        sample_at = caso.sample_at,
                        run_medic = caso.run_medic,
                        symptoms = caso.symptoms.HasValue?caso.symptoms.Value? "Si": "No":"No",
                        symptoms_at = caso.symptoms_at,
                        sample_type = caso.sample_type,
                        epivigila = caso.epivigila,
                        gestation = caso.gestation,
                        gestation_week = caso.gestation_week,
                        observation = caso.observation
                    },
                    paciente = patient,
                    demografico = demographic
                };

                return Ok(retorno);
            }
            catch (Exception e)
            {
                return BadRequest("Computer system error." + e);
            }
        }
    }

    /// <summary>
    /// Representa el caso de sospecha junto con la información del paciente.
    /// </summary>
    public class CasoResponse
    {
        /// <summary>
        /// El caso de sospecha
        /// </summary>
        public Sospecha caso { get; set; }
        
        /// <summary>
        /// EL paciente asociado al caso
        /// </summary>
        public Patients paciente { get; set; }
        
        /// <summary>
        /// Los datos demográficos del paciente
        /// </summary>
        public demographics demografico { get; set; }
    }
}
