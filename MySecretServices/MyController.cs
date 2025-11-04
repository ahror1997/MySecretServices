using AddIn;
using Seagull.BarTender.Print;
using System.Collections.Generic;
using System.Web.Http;
using MySecretServices.Models;
using System;
using MySecretServices.Responses;
using System.Linq;
using System.Web.Http.ModelBinding;

namespace MySecretServices
{
	public class MyController : ApiController
	{
		[HttpPost]
		[Route("api/v1/printer/print")]
		public IHttpActionResult Print([FromBody] PrintProduct product)
		{
			var response = new BaseResponse();

			if (!ModelState.IsValid)
			{
				response.Errors = this.ErrorMessages(ModelState, nameof(product));
				return Ok(response);
			}

			try
			{
				Engine engine = new Engine();
				engine.Start();

				LabelFormatDocument labelFormatDocument = engine.Documents.Open(product.Path);
				labelFormatDocument.SubStrings["name"].Value = product.Name;
				labelFormatDocument.SubStrings["barcode"].Value = product.Barcode;
				labelFormatDocument.SubStrings["price"].Value = product.Price.ToString();
				labelFormatDocument.SubStrings["id"].Value = product.Id;

				for (int i = 0; i < product.Copy; i++)
				{
					labelFormatDocument.Print();
				}

				engine.Stop();

				response.Success = true;
				response.Data = product;
			}
			catch (Exception ex)
			{
				response.Message = ex.Message;
			}

			return Ok(response);
		}

		[HttpGet]
		[Route("api/v1/libra/status")]
		public IHttpActionResult GetStatus(string ip)
		{
			var response = new BaseResponse();

            DrvLP temp = new DrvLP();
            temp.RemoteHost = ip;
            int status = temp.Connect();

            if (status != 0)
            {
                response.Message = "Not connected!";
                return Ok(response);
            }

            try
			{
				temp.Beep();

				response.Success = true;
				response.Data = new { status, ip };
			}
			catch (Exception ex)
			{
				response.Message = ex.Message;
			}
			finally
			{
				temp.Disconnect();
			}

            return Ok(response);
		}

		[HttpPost]
		[Route("api/v1/libra/uploadProducts")]
		public IHttpActionResult UploadProducts(string ip, [FromBody] List<Product> products)
		{
			var response = new BaseResponse();

            DrvLP temp = new DrvLP();
            temp.RemoteHost = ip;
            int status = temp.Connect();

            if (status != 0)
            {
                response.Message = "Not connected!";
                return Ok(response);
            }

            if (!ModelState.IsValid)
			{
				response.Errors = this.ErrorMessages(ModelState, nameof(products));
				return Ok(response);
			}

            try
			{
				foreach (Product product in products)
				{
					temp.Password = 30;
					temp.PLUNumber = product.PLUNumber;
					temp.Price = decimal.Parse(product.Price.ToString());
					temp.ItemCode = product.Id;
					temp.NameFirst = product.Name;
					temp.GroupCode = product.GroupCode;
					temp.GoodsType = product.GoodType;
					temp.Tare = 0;
					temp.NameSecond = "";
					temp.PictureNumber = 0;
					if (temp.SetPLUDataEx() != 0)
					{
						temp.Disconnect();
						break;
					}
				}
				temp.Disconnect();

				response.Success = true;
				response.Data = products;
			}
			catch (Exception ex)
			{
				response.Message = ex.Message;
			}
			finally
			{
				temp.Disconnect();
			}

			return Ok(response);
		}

		[HttpGet]
		[Route("api/v1/libra/clear-goods")]
		public IHttpActionResult ClearGoods(string ip)
		{
            var response = new BaseResponse();

            DrvLP temp = new DrvLP();
            temp.RemoteHost = ip;
            temp.Password = 30;
            int status = temp.Connect();

            if (status != 0)
            {
                response.Message = "Not connected!";
                return Ok(response);
            }

			try
			{
                temp.ClearGoodsDB();
                temp.Beep();
                response.Success = true;
				response.Message = "Goods database cleared.";
            }
            catch (Exception ex)
			{
				response.Message = ex.Message;
			}
			finally
			{
                temp.Disconnect();
            }

			return Ok(response);
        }

        [HttpPost]
		[Route("api/v1/libra/setSettings")]
		public IHttpActionResult SetSettings(string ip, [FromBody] Settings settings)
		{
			var response = new BaseResponse();

			if (!ModelState.IsValid)
			{
				response.Errors = this.ErrorMessages(ModelState, nameof(settings));
				return Ok(response);
			}

			DrvLP temp = new DrvLP();
			temp.RemoteHost = ip;
			temp.Connect();
			temp.Password = 30;
			try
			{
				temp.BCFormat = 7;
				temp.SetBCFormat();
				temp.PointPosition = 0;
				temp.SetPointPosition();
				temp.Date = DateTime.Now;
				temp.SetDate();
				temp.Time = DateTime.Now;
				temp.SetTime();
				temp.PrefixBCPieceGoods = 21;
				temp.SetPiecePrefixBC();
				temp.PrefixBCWeightGoods = 20;
				temp.SetWeightPrefixBC();
				temp.DateFormat = 0;
				temp.SetDateFormat();
				temp.LabelTitle = settings.LabelTitle;
				temp.SetLabelTitle();
				temp.ReclameString = settings.ReclameString;
				temp.SetReclameMessage();
				temp.ShopName = settings.ShopName;
				temp.StringNumber = 2;
				temp.SetShopName();
				temp.Beep();

				response.Success = true;
				response.Data = settings;
			}
			catch (Exception ex)
			{
				response.Message = ex.Message;
			}
			finally
			{
				temp.Disconnect();
			}

			return Ok(response);
		}

		[HttpGet]
		[Route("api/v1/libra/getWeight")]
		public IHttpActionResult GetWeight(string ip)
		{
			var response = new BaseResponse();

			try
			{
				DrvLP temp = new DrvLP();
				temp.RemoteHost = ip;
				temp.Connect();
				double weight = temp.Weight;
				temp.Disconnect();

				response.Success = true;
				response.Data = new { weight, ip };
			}
			catch (Exception ex)
			{
				response.Message = ex.Message;
			}

			return Ok(response);
		}

		private Dictionary<string, string[]> ErrorMessages(ModelStateDictionary modelState, string prefix)
		{
			return modelState.Where(ms => ms.Value.Errors.Any())
				.ToDictionary(
					kvp => kvp.Key.Replace($"{prefix}.", string.Empty).ToLower(),
					kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
				);
		}
	}
}