SharePoint 2016 Filtered Lookup Field

This is SharePoint 2016 port of **SharePoint 2010 Filtered Lookup** provided [here](http://sp2010filteredlookup.codeplex.com/) by Dev4Side, which is also a port of the package provided [here](http://filteredlookup.codeplex.com/) :)

I updated namespaces, pages and SharePoint binaries (to use version 16.0.0.0). Description below is copied from [here](http://filteredlookup.codeplex.com/) and all the features are working perfectly with SharePoint 2016.

# Description
This project creates a custom SharePoint lookup field that offers new functionalities to default SharePoint lookup field by allowing filters to be applied to retrieved data. Applied filters can be either dynamic CAML queries or pre-defined list views residing in source lists.

Below is a few of the features offered by Filtered Lookup field over standard SharePoint Lookup field:  
* Cross-site lookup (all sites within same site collection)
* Filter retrieved data using list views
* Filter retrieved data using dynamic/ad-hoc CAML queries. This means you don't need to create a list view each time you want to apply a lookup filter to source data
* Supports retrieving data recursively using either list views or dynamic queries
* Supports MultiLookup with filtered data
* Same look and feel as default SharePoint Lookup and MultiLookup (i.e. in list forms)

# Screenshot
![filtered_lookup_sp2016.png](https://raw.githubusercontent.com/hasangok/sharepoint-2016-filtered-lookup-field/master/filtered_lookup_sp2016.png)