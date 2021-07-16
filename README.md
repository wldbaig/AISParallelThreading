# AISParallelThreading
The Task:
 
You have access to a library (AisUriProviderNS.dll) which provides an API to list the user’s available files. There’s a AisUriProviderApi.AisUriProvider class and it has the public IEnumerable<Uri> Get(); method, which returns your file’s URIs. Let’s assume that you should always have 10 files at a time on your storage, but those might be totally different ones.
 
Write a console application using C#, which accomplishes the following :

- Output currently available 10 files(file names)
- Store the last 10 files locally and load it on app start-up.
- Delete any unnecessary/old files after syncing.
- Refresh the list automatically every 5 minutes using the following logic: Download the files in a parallel way, having 3 parallel downloads at all times. You shouldn’t run more than 3 tasks at a time and there should not be any download tasks explicitly waiting for each other.
- Errors should be handled gracefully, and displayed to the user via console.
 
