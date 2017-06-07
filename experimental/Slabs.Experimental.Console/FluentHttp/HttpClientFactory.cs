﻿using System;
using System.Collections.Generic;

namespace Slabs.Experimental.ConsoleClient.FluentHttp
{
	/*
	 * todo:
	 *  - HttpClientFactory
	 *		- Default configs
	 *		- Default configs configurable
	 *  - 
	 * 
	 */

	public class HttpClientFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IFluentHttpMiddlewareRunner _middlewareRunner;
		private readonly Dictionary<string, FluentHttpClient> _clientsMap = new Dictionary<string, FluentHttpClient>();

		public HttpClientFactory(IServiceProvider serviceProvider, IFluentHttpMiddlewareRunner middlewareRunner)
		{
			_serviceProvider = serviceProvider;
			_middlewareRunner = middlewareRunner;
		}

		public HttpClientBuilder CreateBuilder(string identifier)
		{
			var builder = new HttpClientBuilder(this);
			builder.SetIdentifier(identifier);
			return builder;
		}

		public void Add(HttpClientBuilder clientBuilder)
		{
			if (clientBuilder == null) throw new ArgumentNullException(nameof(clientBuilder));
			if (Has(clientBuilder.Identifier))
				throw new KeyNotFoundException($"FluentHttpClient '{clientBuilder.Identifier}' is already registered.");

			var clientOptions = clientBuilder.Build();
			// todo: use activator
			var client = new FluentHttpClient(clientOptions, _serviceProvider, _middlewareRunner);

			_clientsMap.Add(clientBuilder.Identifier, client);
		}

		public void Remove(string identity)
		{
			_clientsMap.Remove(identity);
		}

		public FluentHttpClient Get(string identifier)
		{
			if (!_clientsMap.TryGetValue(identifier, out var client))
				throw new KeyNotFoundException($"FluentHttpClient '{identifier}' not registered.");
			return client;
		}

		public bool Has(string identifier) => _clientsMap.ContainsKey(identifier);
	}

	public class HttpClientBuilder
	{
		private readonly HttpClientFactory _httpClientFactory;
		private string _baseUrl;
		private TimeSpan _timeout;
		public string Identifier { get; private set; }
		private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
		private readonly List<Type> _middleware = new List<Type>();

		public HttpClientBuilder(HttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public HttpClientBuilder SetBaseUrl(string url)
		{
			_baseUrl = url;
			return this;
		}

		public HttpClientBuilder SetTimeout(int timeout)
		{
			_timeout = TimeSpan.FromSeconds(timeout);
			return this;
		}
		public HttpClientBuilder SetTimeout(TimeSpan timeout)
		{
			_timeout = timeout;
			return this;
		}

		public HttpClientBuilder AddHeader(string key, string value)
		{
			_headers.Add(key, value);
			return this;
		}

		public HttpClientBuilder SetIdentifier(string identifier)
		{
			Identifier = identifier;
			return this;
		}

		public HttpClientBuilder AddMiddleware<T>()
		{
			_middleware.Add(typeof(T));
			return this;
		}

		public FluentHttpClientOptions Build()
		{
			var options = new FluentHttpClientOptions
			{
				Timeout = _timeout,
				BaseUrl = _baseUrl,
				Identifier = Identifier,
				Headers = _headers,
				Middleware = _middleware
			};

			return options;
		}

		/// <summary>
		/// Register to <see cref="HttpClientFactory"/>, same as <see cref="HttpClientFactory.Add"/>
		/// </summary>
		public HttpClientBuilder Register()
		{
			_httpClientFactory.Add(this);
			return this;
		}
	}

}
