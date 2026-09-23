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
				response.Message = FirstError(ModelState);
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

			if (!ModelState.IsValid)
			{
				response.Message = FirstError(ModelState);
				response.Errors = this.ErrorMessages(ModelState, nameof(products));
				return Ok(response);
			}

			if (products == null || products.Count == 0)
			{
				response.Message = "Products list is empty.";
				return Ok(response);
			}

			DrvLP temp = new DrvLP();
			temp.RemoteHost = ip;
			int status = temp.Connect();

			if (status != 0)
			{
				response.Message = "Not connected!";
				return Ok(response);
			}

			// сбойные товары не прерывают загрузку остальных, а копятся тут
			var failed = new List<object>();

			try
			{
				// после успешного Connect драйвер сам обновляет PLUCount
				int pluCount = temp.PLUCount;

				foreach (Product product in products)
				{
					// PLUCount иногда не заполняется драйвером - тогда полагаемся на код SetPLUDataEx
					if (pluCount > 0 && (product.PLUNumber < 1 || product.PLUNumber > pluCount))
					{
						failed.Add(new
						{
							id = product.Id,
							plu = product.PLUNumber,
							code = -9,
							message = $"PLU {product.PLUNumber} вне диапазона 1..{pluCount}"
						});
						continue;
					}

					string nameFirst, nameSecond;
					SplitName(product.Name, out nameFirst, out nameSecond);

					temp.Password = 30;
					temp.PLUNumber = product.PLUNumber;
					temp.Price = (decimal)product.Price;
					temp.ItemCode = product.Id;
					temp.NameFirst = nameFirst;
					temp.NameSecond = nameSecond;
					temp.GroupCode = product.GroupCode;
					temp.GoodsType = product.GoodType;
					temp.Tare = 0;
					temp.PictureNumber = 0;

					if (temp.SetPLUDataEx() != 0)
					{
						failed.Add(new
						{
							id = product.Id,
							plu = product.PLUNumber,
							code = temp.ResultCode,
							message = temp.ResultCodeDescription
						});
					}
				}

				int written = products.Count - failed.Count;
				response.Success = failed.Count == 0;
				response.Data = new { written, failed };
				response.Message = failed.Count == 0
					? $"Written {written} of {products.Count}"
					: $"Written {written} of {products.Count}, failed {failed.Count}";
			}
			catch (Exception ex)
			{
				// COM-исключение посреди цикла не должно терять уже собранный список сбойных
				response.Data = new { written = products.Count - failed.Count, failed };
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
				response.Message = FirstError(ModelState);
				response.Errors = this.ErrorMessages(ModelState, nameof(settings));
				return Ok(response);
			}

			DrvLP temp = new DrvLP();
			temp.RemoteHost = ip;
			int status = temp.Connect();

			if (status != 0)
			{
				response.Message = "Not connected!";
				return Ok(response);
			}

			temp.Password = 30;
			try
			{
				temp.BCFormat = 7;
				temp.SetBCFormat();
				temp.PointPosition = 0;
				temp.SetPointPosition();
				// без этого весы решают формат префикса сами (может оказаться групповой код)
				temp.PrefixBCType = 2;
				temp.SetPrefixBCType();
				temp.PrefixBCPieceGoods = 21;
				temp.SetPiecePrefixBC();
				temp.PrefixBCWeightGoods = 20;
				temp.SetWeightPrefixBC();
				temp.Date = DateTime.Now;
				temp.SetDate();
				temp.Time = DateTime.Now;
				temp.SetTime();
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
					kvp => kvp.Value.Errors.Select(e => ErrorText(e)).ToArray()
				);
		}

		// первая ошибка валидации коротким текстом для поля message
		private static string FirstError(ModelStateDictionary modelState)
		{
			foreach (var state in modelState.Values)
			{
				foreach (var error in state.Errors)
				{
					string text = ErrorText(error);
					if (!string.IsNullOrEmpty(text))
					{
						return text;
					}
				}
			}

			return "Invalid request.";
		}

		// ошибки десериализации Json.NET приходят с пустым ErrorMessage и текстом в Exception
		private static string ErrorText(ModelError error)
		{
			if (!string.IsNullOrEmpty(error.ErrorMessage))
			{
				return error.ErrorMessage;
			}

			return error.Exception != null ? error.Exception.Message : string.Empty;
		}

		// NameFirst/NameSecond у драйвера ограничены 28 символами каждое
		private static void SplitName(string name, out string first, out string second)
		{
			name = name ?? string.Empty;
			first = name.Length > 28 ? name.Substring(0, 28) : name;
			second = name.Length > 28 ? name.Substring(28, Math.Min(28, name.Length - 28)) : string.Empty;
		}
	}
}