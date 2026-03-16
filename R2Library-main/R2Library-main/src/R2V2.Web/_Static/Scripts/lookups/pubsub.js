var R2V2 = R2V2 || { };

/* ==========================================================================
    Pub/Sub Mappings
   ========================================================================== */
R2V2.PubSubMappings = {
    History: {
        Subscribe: 'history.subscribe',
        Set: 'history.set',
        Add: 'history.add',
        Remove: 'history.remove'
    },
    
    Filtering: {
        Success: 'results.success',
        Error: 'results.error'
    },
    
    Results: {
        Set: 'results.set',
        Get: 'results.get'
    },
    
    Resource: {
        Changed: 'resource.changed',
        Success: 'resource.success',
        Error: 'resource.error',
        TimeoutExpired: 'resource.timeout-expired'
    },

    Log: {
    	Changed: 'log.changed',
    	Success: 'log.success',
    	Error: 'log.error'
    },
		
    UserContentFolder: {
        Updated: 'user-content-folder.updated'
    },
    
    UserContentItem: {
        Removed: 'user-content-item.removed',
        Moved: 'user-content-item.moved'
    },
    
    Menu: {
        Opened: 'menu.opened',
        Closed: 'menu.closed'
    },
    
    Tag: 'tag.event',

    Selection: {
        Changed: 'selection.changed'
    },

    InlineResourceView: {
        Changed: 'inline-resource-view.changed'
    }    
}