using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine.Rendering.Universal.Internal;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AccountManager : MonoBehaviour
{
	public static AccountManager Instance { get; private set; }

	public static string LocalApiUrl { get; } = "https://localhost:7267";
	public static string PublicApiUrl { get; } = "https://j1p3lu2-herkansing-hd-d7gaged3fkcwhhgt.canadacentral-01.azurewebsites.net";
	public static string ApiUrl { get; } = PublicApiUrl;

	[SerializeField]
	public GameObject EmailInputField;

	[SerializeField]
	public GameObject PasswordInputField;

	[SerializeField]
	public GameObject StatusTextBox;

	[SerializeField]
	public string NextSceneName;

	private string _bearerToken;

	public async void OnLoginButtonPress()
	{
		bool success = await Login(
			EmailInputField.GetComponent<TMP_InputField>().text,
			PasswordInputField.GetComponent<TMP_InputField>().text
		);
	}

	public async void OnRegisterButtonPress()
	{
		string email = EmailInputField.GetComponent<TMP_InputField>().text;
		string password = PasswordInputField.GetComponent<TMP_InputField>().text;

		bool success = await Register(email, password);

		if (success)
		{
			success = await Login(email, password);
		}
	}

	class RegisterErrorMessage
	{
		public string requestError;
		public string statusText;
		
		public RegisterErrorMessage(string request, string status)
		{
			requestError = request;
			statusText = status;
		}

	}

	private static RegisterErrorMessage[] errorMessages = new RegisterErrorMessage[] {
		new ("DuplicateUserName", "A user with that username already exists."),
		new ("PasswordTooShort", "Password must be at least 10 characters."),
		new ("PasswordRequiresLower", "Password must contain at least one lowercase letter."),
		new ("PasswordRequiresUpper", "Password must contain at least one uppercase letter."),
		new ("PasswordRequiresDigit", "Password must contain at least one numeric character."),
		new ("PasswordRequiresNonAlphanumeric", "Password must contain at least one non-alphanumeric character.")
	};

	private async Task<bool> Register(string email, string password)
	{
		string url = ApiUrl + "/account/register";
		string content =
			$"{{" +
				$"\"email\": \"{email}\"," +
				$"\"password\": \"{password}\"" +
			$"}}";


		TMP_Text statusText = StatusTextBox.GetComponent<TMP_Text>();
		statusText.color = Color.black;
		statusText.text = "Registering...";

		UnityWebRequest request = await PerformApiCall(url, "POST", content, null);
		if (request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log(request.downloadHandler.text);
			return true;
		}
		else
		{
			JObject errorJson = JObject.Parse(request.downloadHandler.text);
			int status = errorJson["status"].Value<int>();
			if (status == 400)
			{
				JObject errors = errorJson["errors"].Value<JObject>();
				bool errorLogged = false;
				foreach (RegisterErrorMessage message in errorMessages)
				{
					if (errors.ContainsKey(message.requestError))
					{
						statusText.color = Color.red;
						statusText.text = message.statusText;
						errorLogged = true;
						break;
					}
				}

				if (!errorLogged)
				{
					statusText.color = Color.red;
					statusText.text = request.downloadHandler.text;
				}
			}

			return false;
		}
	}

	private async Task<bool> Login(string email, string password)
	{
		string url = ApiUrl + "/account/login";
		string content =
			$"{{" +
				$"\"email\": \"{EmailInputField.GetComponent<TMP_InputField>().text}\"," +
				$"\"password\": \"{PasswordInputField.GetComponent<TMP_InputField>().text}\"" +
			$"}}";
		TMP_Text statusText = StatusTextBox.GetComponent<TMP_Text>();
		statusText.color = Color.black;
		statusText.text = "Logging in...";
		UnityWebRequest request = await PerformApiCall(url, "POST", content, null);

		if (request.result == UnityWebRequest.Result.Success)
		{
			JObject resultJson = JObject.Parse(request.downloadHandler.text);
			string tokenType = resultJson["tokenType"].Value<string>();
			if (tokenType != "Bearer")
			{
				statusText.color = Color.red;
				statusText.text = "Unknown token type.";
				return false;
			}

			string bearerToken = resultJson["accessToken"].Value<string>();
			if (string.IsNullOrEmpty(bearerToken))
			{
				statusText.color = Color.red;
				statusText.text = "Invalid token.";
				return false;
			}
			
			statusText.color = Color.darkGreen;
			statusText.text = "Success.";

			SceneManager.LoadScene(NextSceneName);

			_bearerToken = bearerToken;
			return true;
		} else if (request.result == UnityWebRequest.Result.ConnectionError)
		{
			statusText.color = Color.red;
			statusText.text = "Connection error.";
			Debug.LogError(request.downloadHandler.text);
			return false;
		} else if (request.result == UnityWebRequest.Result.DataProcessingError) {
			statusText.color = Color.red;
			statusText.text = "Error while processing data.";
			Debug.LogError(request.downloadHandler.text);
			return false;
		} else if (request.result == UnityWebRequest.Result.ProtocolError)
		{
			statusText.color = Color.red;
			statusText.text = "Failed to authenticate you.";
			Debug.LogError(request.downloadHandler.text);
			return false;
		}

		return false;
	}

    public async Task<UnityWebRequest> PerformApiCall(string url, string method, string? jsonData, string? token)
	{
		UnityWebRequest request = new UnityWebRequest(url, method);
		if (!string.IsNullOrEmpty(jsonData))
		{
			byte[] jsonBinary = Encoding.UTF8.GetBytes(jsonData);
			request.uploadHandler = new UploadHandlerRaw(jsonBinary);
		}

		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		if (!string.IsNullOrEmpty(token))
		{
			request.SetRequestHeader("Authorization", "Bearer " + token);
		}

		await request.SendWebRequest();
		return request;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	public string GetBearerToken()
	{
		return _bearerToken;
	}
}
