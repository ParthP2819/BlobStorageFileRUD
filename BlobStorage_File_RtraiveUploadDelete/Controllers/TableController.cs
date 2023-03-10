using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using BlobStorage_File_RtraiveUploadDelete.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace BlobStorage_File_RtraiveUploadDelete.Controllers
{
    public class TableController : Controller
    {
        public IActionResult Index()
        {
            TableStorage abc= new TableStorage();

            return View(abc);
        }

        [HttpPost]
        public IActionResult CreateT(string id) 
        {
            //TableServiceClient client = "abc";
            //TableItem table = client.CreateTable(id); 
            return View();
        }
    }
}
