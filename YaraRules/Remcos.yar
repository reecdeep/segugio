rule Remcos {
   meta:
      description = "Remcos RAT malware"
      author = "reecdeep"
      name = "Remcos"
   strings:
      $remcos = {52 65 6D 63 6F 73 20 76} //remcos v
      $breakingsec = {A9 20 42 72 65 61 6B 69 6E 67 53 65 63 75 72 69 74 79 2E 6E 65 74} //© BreakingSecurity.net
      $settings = {00 00 00 53 45 54 54 49 4E 47 53 00 00 00 00}  //???SETTINGS????
      $geo = {00 67 00 65 00 6F 00 70 00 6C 00 75 00 67 00 69 00 6E 00 2E 00 6E 00 65 00 74 00}  //?g?e?o?p?l?u?g?i?n?.?n?e?t
      $watchdog = {57 61 74 63 68 64 6F 67 20 6D 6F 64 75 6C 65}  //Watchdog module

      
   condition:
      all of them
     
}

