global using System;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Collections.ObjectModel;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Runtime.InteropServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading;
global using System.Threading.Tasks;
global using Accelerate;
global using Accelerate.Commands;
global using Accelerate.Exceptions;
global using Accelerate.Repositories;
global using Accelerate.Settings;
global using Accelerate.Verbs;
global using CliWrap;
global using CliWrap.Buffered;
global using CommandLine;
global using Markdig;
global using Markdig.Renderers.Roundtrip;
global using Markdig.Syntax;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;