

Update tUserOption
set vchUserOptionDescription = 'New resources are now active<br />Subject: <i>R2 Library New Titles Available</i>'
where vchUserOptionCode = 'NewResource';
Update tUserOption
set vchUserOptionDescription = 'New edition of a purchased resource is active<br />Subject: <i>R2 Library New Editions Now Available</i><br />Subject: <i>R2 Library New PDA Editions Now Available</i>'
where vchUserOptionCode = 'NewEdition';
Update tUserOption
set vchUserOptionDescription = 'Shopping cart reminder (1 week, 2 weeks, 1 month)<br />Subject: <i>R2 Library Shopping Cart Report</i>'
where vchUserOptionCode = 'CartRemind';
Update tUserOption
set vchUserOptionDescription = 'Purchased forthcoming resources are now active<br />Subject: <i>R2 Library Purchased Titles Now Available</i>'
where vchUserOptionCode = 'ForthcomingPurchase';
Update tUserOption
set vchUserOptionDescription = 'Medical DCT and DCT Essential updates<br />Subject: <i>R2Library Medical DCT and DCT Essentials Update</i>'
where vchUserOptionCode = 'DctMedical';
Update tUserOption
set vchUserOptionDescription = 'Nursing DCT and DCT Essential updates<br />Subject: <i>R2Library Nursing DCT and DCT Essentials Update</i>'
where vchUserOptionCode = 'DctNursing';
Update tUserOption
set vchUserOptionDescription = 'Allied Health DCT and DCT Essential updates<br />Subject: <i>R2Library: Allied Health DCT and DCT Essentials Update</i>'
where vchUserOptionCode = 'DctAlliedHealth';
Update tUserOption
set vchUserOptionDescription = 'PDA trigger events<br />Subject: <i>R2 Library PDA Title Added To Cart</i><br />Subject: <i>R2 Library PDA Title Removed from Cart</i>'
where vchUserOptionCode = 'PdaAddToCart';
Update tUserOption
set vchUserOptionDescription = 'PDA history report<br />Subject: <i>R2 Library PDA History Report</i>'
where vchUserOptionCode = 'PdaReport';
Update tUserOption
set vchUserOptionDescription = 'Purchased and PDA resources have become archived<br />Subject: <i>R2 Library Archived Titles</i>'
where vchUserOptionCode = 'ArchivedAlert';
Update tUserOption
set vchUserOptionDescription = '"Ask Your Librarian" requests<br />Subject: <i>Ask Your Librarian</i>'
where vchUserOptionCode = 'LibrarianAlert';
Update tUserOption
set vchUserOptionDescription = 'Recommendations from Expert Reviewers<br />Subject: <i>R2 Library Expert Reviewer User Recommendations</i>'
where vchUserOptionCode = 'ExpertReviewRecommend';
Update tUserOption
set vchUserOptionDescription = 'Requests for role change from User to Expert Reviewer<br />Subject: <i>R2 Library Expert Reviewer User Request</i>'
where vchUserOptionCode = 'ExpertReviewUserRequest';
Update tUserOption
set vchUserOptionDescription = 'Report containing Institutions who''s Annual Maintenance fee is due<br />Subject: <i>R2 Library Annual Maintenance Fee Report</i>'
where vchUserOptionCode = 'AnnualMaintenanceFee';
Update tUserOption
set vchUserOptionDescription = 'Activity Summary and News<br />Subject: <i>Your Library’s R2 Activity Summary and News</i>'
where vchUserOptionCode = 'Dashboard';
Update tUserOption
set vchUserOptionDescription = 'User is denied access to a resource<br /> Subject: <i>R2 Library Turnaways in the Last Day</i>'
where vchUserOptionCode = 'AccessDenied';
