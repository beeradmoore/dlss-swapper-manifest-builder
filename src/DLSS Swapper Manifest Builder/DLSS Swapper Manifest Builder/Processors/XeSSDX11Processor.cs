using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class XeSSDX11Processor : DLLProcessor
{
	public override string NamePath => "xess_dx11";
	public override string ExpectedDLLName => "libxess_dx11.dll";
	public override string[] ValidFileDescriptions => new string[]
	{
	"XeSS SDK"
	};
	public override string[] ExpectedPrefix => new string[]
	{
	"bin/",
	"/",
	"Binaries/ThirdParty/Win64/",
	};
	public override string[] ExpectedDevPrefix => new string[]
	{

	};
	
	public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>();

	public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
	{
	};
}