using AddIn;
using Seagull.BarTender.Print;
using System.Collections.Generic;
using System;
using System.Web.Http;
using MySecretServices.Models;
using System.IO;

namespace MySecretServices
{
	public class MyController : ApiController
	{
		[HttpGet]
		[Route("api/hello")]
		public IHttpActionResult GetHello()
		{
			return Ok("hello world!");
		}

		[HttpPost]
		[Route("api/v1/printer/print")]
		public IHttpActionResult Print([FromBody] PrintProduct product)
		{
			//string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			//string labelPath = Path.Combine(userDocumentsPath, "label.btw");

			//string labelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "label.btw");

			Engine engine = new Engine();
			engine.Start();

			LabelFormatDocument labelFormatDocument = engine.Documents.Open(product.Path);
			labelFormatDocument.SubStrings["name"].Value = product.Name;
			labelFormatDocument.SubStrings["barcode"].Value = product.Barcode;
			labelFormatDocument.SubStrings["price"].Value = product.Price;
			labelFormatDocument.SubStrings["id"].Value = product.Id;

			for (int i = 0; i < product.Copy; i++)
			{
				labelFormatDocument.Print();
			}

			engine.Stop();


			return Ok(product);
		}

		[HttpGet]
		[Route("api/v1/libra/status")]
		public IHttpActionResult GetStatus(string ip)
		{
			DrvLP temp = new DrvLP();
			temp.RemoteHost = ip;
			int status = temp.Connect();

			// logic

			temp.Disconnect();

			return Ok(status);
		}

		[HttpPost]
		[Route("api/v1/libra/uploadProducts")]
		public IHttpActionResult UploadProducts(string ip, [FromBody] List<Product> products)
		{
			return Ok(products);
		}
	}
}
