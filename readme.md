# Crolow.Umbraco.OpenAI
Crolow.Umbraco.OpenAI is a package to help you creating content within umbraco using OpenAI.
It consists into two part : 
- a Dashboard that will help you to perform actions on your complete tree
- a content app that enables you to perform actions on each content item.

Each action is configurable into the appsettings of your application. They mainly configure what roles you can use : "Writer, poet, rapper, scientist", and mappings between the content to fetch and send to the OpenAI service, and the output result to updated or new content. Check the appsettings.openai.json file

Actual available actions are :
- Describe an image : It generates a description, tags, and hashtags from an image and a hint field of the content
- Summarize : It generates a summary, a teaser, and tags from the content.
- Translate : Translate your content from one language to another.
- Correct : Correct your content
- Create hints : Some code I used to generate the complete content of the website.

It has been tested on https://crolow.eu => it suits to my need, but it can still need more customisation and fixes.

To use the package :
- open the solution in Visual Studio:
  - Pack the Crolow.OpenAi.Umbraco
  - Copy the create nuget package into a local repository
  - Install the new package
  - Update your appsettings from your solution
- Copy the projects into your solution
- Refer the generated DLLS into your solution

For more info : crolow@outlook.com
