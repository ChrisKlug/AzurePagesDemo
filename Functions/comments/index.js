var azure = require('azure-storage')

module.exports = async function (context, req) {
    const connectionstring = isStaging(req) ? process.env.CUSTOMCONNSTR_STAGING : process.env.CUSTOMCONNSTR_PRODUCTION;
    if (req.method.toUpperCase() == "GET")
        context.res = await getComments(connectionstring, req.query["postId"]);
    else
        context.res = await addComments(connectionstring, { id: req.body.postId, body: req.body.body, commenter: req.body.commenter });
}

async function getComments(connectionstring, postId)
{
    return new Promise(async (resolve, reject) => {
        try
        {
            const tableService = azure.createTableService(connectionstring);
            tableService.doesTableExist("comments", (error, result, response) => {
                if (!result || !result.exists) {
                    resolve({ status: 200, body: JSON.stringify([]) });
                    return;
                }
    
                getCommentsRecursively(tableService,  postId, [], undefined).then(comments => {;
                    resolve({ status: 200, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(comments.map(x => ({ body: x.Body._, commenter: x.Commenter._, timestamp: x.Timestamp._ }))) });
                }, error => {
                    resolve({ status: 500, body: "Failed to retrieve comments" });
                })
            })
        } 
        catch (e) 
        {
            reject(e);
        }
    });
}

async function addComments(connectionstring, post)
{
    return new Promise(async (resolve, reject) => {
        try
        {
            const tableService = azure.createTableService(connectionstring);
            tableService.createTableIfNotExists('comments', (error, result, response) => {
                if (error) {
                    resolve({ status: 500, body: "Could not create table - " + error  });
                    return;
                }

                var rowKey = new Date().toString();
                var entGen = azure.TableUtilities.entityGenerator;
                var entity = {
                    PartitionKey: entGen.String(post.id),
                    RowKey: entGen.String(rowKey),
                    Body: entGen.String(post.body),
                    Commenter: entGen.String(post.commenter)
                };
                tableService.insertEntity('comments', entity, function(error, result, response) {
                    if (error) {
                        resolve({ status: 500, body: "Could not create comment - " + error  });
                        return;
                    }
                
                    resolve({ status: 200, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ body: post.body, commenter: post.commenter, timestamp: rowKey })  }); 
                    return;
                });
            });
            
        } 
        catch (e) 
        {
            reject(e);
        }
    });
}

function isStaging(request) {
    if (request.headers["Referer"]) {
        return request.headers["Referer"].indexOf("staging") >= 0;
    }
    return true;
}

async function getCommentsRecursively(tableService, postId, entries, token) {
    return new Promise(async (resolve, reject) => {
        var query = new azure.TableQuery().where('PartitionKey eq ?', postId);
        tableService.queryEntities("comments", query, token, (error, result, response) => {
            if (error) {
                throw error;
            }
            entries.push(...result.entries);
            if (result.continuationToken) {
                getCommentsRecursively(tableService, postId, entries, result.continuationToken).then(x => {
                    entries.push(...result.entries);
                    resolve(entries);
                })
            } else {
                resolve(entries);
            }
        })
    })
}