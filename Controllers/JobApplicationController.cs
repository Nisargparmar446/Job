using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Job.Abstraction.ViewDataModels;
using Job.Abstraction.Services;
using Job.Common;
using Job.Abstraction;
using Microsoft.AspNetCore.Mvc.Rendering;
using Job.Services;
using Job.Common;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Reflection;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Job.Controllers
{
    public class JobApplicationController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IAccountServices _iAccountService;
        private readonly IJobApplicationServices _iJobApplicationService;
        private readonly ClaimsPrincipal _claimPincipal;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IdropdownMasterService _dropdownMasterService;
        public JobApplicationController(IConfiguration config, IAccountServices iAccountService, IJobApplicationServices iJobApplicationService, IdropdownMasterService idropdownMasterService, IHttpContextAccessor httpContextAccessor)
        {

            _config = config;
            _iAccountService = iAccountService;
            _iJobApplicationService = iJobApplicationService;
            _dropdownMasterService = idropdownMasterService;
            _httpContextAccessor = httpContextAccessor;
            _claimPincipal = _httpContextAccessor.HttpContext.User ;
        }
        public IActionResult GetJobApplication()
        {
            BindDropDown(1);
            return View();
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetJobApplicationList(DashBoard dashBoard)
        {
            int jobApplicationId = 0;
            long userId = Convert.ToInt64(_claimPincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                dashBoard.UserId = userId;
                var modelList = await _iJobApplicationService.GetJobApplicationList(dashBoard);
                return View("JobApplicationDashboard",modelList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }
        //[HttpGet]
        //public async Task<IActionResult> GetJobApplication(string strJobApplicationId = "")
        //{
        //    int JobApplicationId = 0;int UserId = 0;
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(strJobApplicationId))
        //        {
        //            JobApplicationId = Convert.ToInt32(strJobApplicationId);
        //        }

        //        UserId = UserId == 0 ? Convert.ToInt32(User.FindFirstValue("UserId")) : UserId;
        //        JobApplicationDetails jobApplicationDetails = await _iJobApplicationService.GetApplication(ApplicantId, ApplicationId);
        //        ChargesModelData chargesModelData = new ChargesModelData();
        //        chargesModelData.ApplicationId = ApplicationId;
        //        chargesModelData.ApplicantId = ApplicantId;
        //        //model.ObjChargesModel = await _applicationService.GetCharges(chargesModelData);
        //        //model.ObjChargesModel.AddRange(cModel);
        //        //model.objApplicationViewModel.FromDateTime = Convert.ToDateTime(model.objApplicationViewModel.FromDateTime).ToString("dd/MM/yyyy HH:mm:ss").Replace(' ', 'T');
        //        //model.objApplicationViewModel.ToDateTime = Convert.ToDateTime(model.objApplicationViewModel.ToDateTime).ToString("yyyy-MM-dd HH:mm:ss").Replace(' ', 'T');
        //        var lstAttachments = await _applicationService.GetAttachmentDetails(ApplicationId, ApplicantId);

        //        if (lstAttachments.Count() > 0)
        //        {
        //            model.ObjAttachmentsData = lstAttachments.FirstOrDefault(x => x.ApplicationTypeId == 1);
        //        }

        //        if (model.objApplicationViewModel != null)
        //        {
        //            BindDropDown(Convert.ToInt32(model.objApplicationViewModel.LocationForHangerId));
        //        }
        //        else
        //        {
        //            BindDropDown(0);
        //        }
        //        if (model.objApplicationViewModel == null)
        //        {
        //            model.objApplicationViewModel = new ApplicationViewModelData();
        //            model.objApplicationViewModel.ApplicantId = ApplicantId;
        //        }
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> AddUpdateJobApplicationDetails(JobApplicationDetails jobApplicationDetails)
        {
            try
            {
                ResponseMessage regResponse = new ResponseMessage();
                var ObjEducationDetailsList = new List<EducationDetails>();
                var ObjWorkExpDetailsList = new List<WorkExpDetails>();
                if (ModelState.IsValid)
                {
                    jobApplicationDetails.IPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                    jobApplicationDetails.UserId = Convert.ToInt32(User.FindFirstValue("UserId"));

                    foreach (var item in jobApplicationDetails.objEducationDetails)
                    {
                        var ObjEducation = new EducationDetails();
                        if (item.IsDeleted == false)
                        {
                            ObjEducation.EducationId = item.EducationId;
                            ObjEducation.University = item.University;
                            ObjEducation.PassoutYear = item.PassoutYear;
                            ObjEducation.Percentage = item.Percentage;
                            ObjEducationDetailsList.Add(ObjEducation);
                        }
                    }
                    var dtData = ToDataTable(ObjEducationDetailsList);
                    jobApplicationDetails.dtEducationDetails = dtData;
                    foreach (var item in jobApplicationDetails.objWorkExpDetails)
                    {
                        var ObjWorkExp = new WorkExpDetails();
                        if (item.IsDeleted == false)
                        {
                            ObjWorkExp.Company = item.Company;
                            ObjWorkExp.Designation = item.Designation;
                            ObjWorkExp.ExpFromDate = item.ExpFromDate;
                            ObjWorkExp.ExpToDate = item.ExpToDate;
                            ObjWorkExpDetailsList.Add(ObjWorkExp);
                        }
                    }
                    var dtWork = ToDataTable(ObjWorkExpDetailsList);
                    jobApplicationDetails.dtWorkExpDetails = dtWork;
                    regResponse = await _iJobApplicationService.AddUpdateJobApplicationDetails(jobApplicationDetails);
                    if (regResponse != null)
                    {
                        string errorMsg = regResponse.Msg == null ? "Somthing went wrong please try again." : regResponse.Msg;

                        if (regResponse != null && regResponse.ErrorCode == 0)
                        {
                            ModelState.Clear();
                            TempData["Message"] = CommonUtils.ConcatString(errorMsg, Convert.ToString((int)EnumLookup.ResponseMsgType.success), "||");
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            TempData["Message"] = CommonUtils.ConcatString(errorMsg, Convert.ToString((int)EnumLookup.ResponseMsgType.warning), "||");
                            ModelState.Clear();
                            return RedirectToAction("AddUpdateJobApplicationDetails", "JobApplication");
                        }
                    }
                    else
                    {
                        TempData["Message"] = CommonUtils.ConcatString("Somthing went wrong please try after sometime.", Convert.ToString((int)EnumLookup.ResponseMsgType.error), "||");
                        return RedirectToAction("AddUpdateJobApplicationDetails", "JobApplication");
                    }
                }
                else
                {
                    TempData["Message"] = CommonUtils.ConcatString("Somthing went wrong please try after sometime.", Convert.ToString((int)EnumLookup.ResponseMsgType.error), "||");
                    return RedirectToAction("AddUpdateJobApplicationDetails", "JobApplication");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void BindDropDown(int Type = 0)
        {
            ViewBag.EducationType = _dropdownMasterService.GetDropDownMaster(Convert.ToInt32(CommonEnums.DropdownMasterType.EducationType), 0)
                    .Select(c => new SelectListItem() { Text = c.Text, Value = c.Value }).ToList();
                    
            //ViewBag.EducationType = new List<SelectListItem>
            //    {
            //        new SelectListItem { Text = "SSC", Value = "1" },
            //        new SelectListItem { Text = "HSC", Value = "2" }
            //    };

            //ViewBag.LanguageType = _dropdownMasterService.GetDropDownMaster(Convert.ToInt32(CommonEnums.DropdownMasterType.LanguageType), 0)
            //        //.Select(c => new SelectListItem() { Text = c.Text, Value = c.Value }).ToList();
            //        .Select(c => new SelectListItem() { Text = "Hindi", Value = "1" }).ToList();

            
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
}
