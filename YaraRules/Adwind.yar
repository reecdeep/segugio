rule Adwind_RAT
{
	meta:
    name = "Adwind_RAT"
		author = "reecdeep <reecdeep@gmail.com>"
		date = "10.04.2024"
		description = "Adwind RAT execution and malicious stage"
		hash = "E5E95F3781E28A01555864D072CDC70BC94190170AAED836148BF740CAB63755" //SIPARIS_LISTESI_02.01.2024.BMP.JAR

	strings:
		$adwind = {6F 70 63 69 6F 6E 65 73 2F 41 64 77 69 6E 64} // opciones/Adwind
		$adwind2 = {6F 70 63 69 6F 6E 65 73 2F 49 6E 73 74 61 6C 61 64 6F 72} //opciones/Instalador
		$adwind3 = {6F 70 63 69 6F 6E 65 73 2F 49 6E 66 6F 72 6D 61 63 69 6F 6E} // opciones/Informacion
		$conf = {63 6F 6E 66 69 67 2E 78 6D 6C} //config.xml
		$prop_open = {3C 70 72 6F 70 65 72 74 69 65 73 3E} //begin configuration
		$prop_close = {3C 2F 70 72 6F 70 65 72 74 69 65 73 3E} //end configuration
		

	condition:
		all of them
}