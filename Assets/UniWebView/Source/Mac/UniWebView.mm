//
//  UniWebView.m
//  UniWebView
//
//  Created by 王 巍 on 13-9-23.
//  Copyright (c) 2013年 王 巍. All rights reserved.
//

#import <WebKit/WebKit.h>
#import <unistd.h>
#import <Carbon/Carbon.h>
#import <OpenGL/gl.h>

typedef void (* UnityCommandCallback)(const char *gameObjectName, const char *methodName, const char *parameter);

static UnityCommandCallback lastCallback = NULL;

extern "C" {
    void _ConnetCallback(UnityCommandCallback callbackName);
    void _CallMethod(const char *gameObjectName, const char *methodName, const char *parameter);
}


void _ConnetCallback(UnityCommandCallback callbackName) {
    lastCallback = callbackName;
}

void _CallMethod(const char *gameObjectName, const char *methodName, const char *parameter) {
    if (lastCallback != NULL) {
        lastCallback(gameObjectName, methodName, parameter);
    }
}

@interface UniWebViewManager : NSObject
{
    NSMutableDictionary *_webViewDic;
    NSMutableDictionary *_webViewBitMapDic;
    NSMutableDictionary *_webViewTextureIdDic;
    NSMutableDictionary *_webViewCurrenUrlDic;
    NSMutableDictionary *_webViewResponseScheme;
    NSString *_customUserAgent;
}
@end

@interface UniWebViewManager()
@property (nonatomic, copy) NSString *customUserAgent;
@end

@implementation UniWebViewManager

@synthesize customUserAgent = _customUserAgent;

+ (UniWebViewManager *) sharedManager
{
    static dispatch_once_t once;
    static UniWebViewManager *instance;
    dispatch_once(&once, ^ { instance = [[UniWebViewManager alloc] init]; });
    return instance;
}

-(instancetype) init {
    self = [super init];
    if (self) {
        _webViewDic = [[NSMutableDictionary alloc] init];
        _webViewBitMapDic = [[NSMutableDictionary alloc] init];
        _webViewTextureIdDic = [[NSMutableDictionary alloc] init];
        _webViewCurrenUrlDic = [[NSMutableDictionary alloc] init];
        _webViewResponseScheme = [[NSMutableDictionary alloc] init];
    }
    return self;
}

-(void) addManagedWebView:(WebView *)webView forName:(NSString *)name
{
    if (![_webViewDic objectForKey:name]) {
        [_webViewDic setObject:webView forKey:name];

        NSMutableArray *schemes = [NSMutableArray arrayWithObject:@"uniwebview"];
        [_webViewResponseScheme setObject:schemes forKey:name];
    } else {
//        NSLog(@"Duplicated name. Something goes wrong: %@", name);
    }
    
}

-(void) addManagedWebViewName:(NSString *)name insets:(NSEdgeInsets)insets screenSize:(CGSize)size
{
    WebView *webView = [[WebView alloc] init];
//    NSLog(@"addManagedWebViewName %@",webView);
    [webView setPolicyDelegate:self];
    [webView setFrameLoadDelegate:self];
    
    if (self.customUserAgent.length != 0) {
        [webView setCustomUserAgent:self.customUserAgent];
    }
    
    [self changeWebView:webView insets:insets screenSize:size];
    webView.hidden = YES;
    [self addManagedWebView:webView forName:name];
    [webView release];
}

-(void) changeWebViewName:(NSString *)name insets:(NSEdgeInsets)insets screenSize:(CGSize)size
{
    WebView *webView = [_webViewDic objectForKey:name];
    [self changeWebView:webView insets:insets screenSize:size];
}

-(void) changeWebView:(WebView *)webView insets:(NSEdgeInsets)insets screenSize:(CGSize)size
{
    NSRect f = NSMakeRect(0, 0, size.width - insets.left - insets.right, size.height - insets.top - insets.bottom);
    webView.frame = f;
}

-(void) webviewName:(NSString *)name beginLoadURL:(NSString *)urlString
{
    WebView *webView = [_webViewDic objectForKey:name];
//    NSLog(@"%@ beginLoadURL %@",webView, urlString);
    NSURL *url = [NSURL URLWithString:urlString];
    NSURLRequest *request = [NSURLRequest requestWithURL:url];
    [webView.mainFrame loadRequest:request];
}

-(void) webViewNameReload:(NSString *)name {
    WebView *webView = [_webViewDic objectForKey:name];
    [webView reload:self];
}

-(void) webViewNameStop:(NSString *)name {
    WebView *webView = [_webViewDic objectForKey:name];
    [webView stopLoading:self];
}

-(void) webViewNameCleanCache:(NSString *)name {
    WebView *webView = [_webViewDic objectForKey:name];
    [[NSURLCache sharedURLCache] removeCachedResponseForRequest:webView.mainFrame.dataSource.request];
}

-(void) webViewNameCleanCookie:(NSString *)name forKey:(NSString *)key {
    
    NSHTTPCookie *cookie;
    NSHTTPCookieStorage *cookieJar = [NSHTTPCookieStorage sharedHTTPCookieStorage];
    
    if (key.length) {
        NSLog(@"Removing cookie for %@", key);
        for (cookie in [cookieJar cookies]) {
            if ([cookie.name isEqualToString:key]) {
                [cookieJar deleteCookie:cookie];
                NSLog(@"Found cookie for %@, removed.", key);
            }
        }
    } else {
        NSLog(@"Removing all cookies");
        for (cookie in [cookieJar cookies]) {
            [cookieJar deleteCookie:cookie];
        }
    }

    [[NSUserDefaults standardUserDefaults] synchronize];
}

-(void) webViewName:(NSString *)name show:(BOOL)show
{
    WebView *webView = [_webViewDic objectForKey:name];
//    NSLog(@"%@ show %d",webView, show);
    webView.hidden = !show;
}

-(void) removeWebViewName:(NSString *)name
{
//    NSLog(@"removeWebViewName %@",name);
    [_webViewDic removeObjectForKey:name];
    [_webViewBitMapDic removeObjectForKey:name];
    [_webViewTextureIdDic removeObjectForKey:name];
    [_webViewCurrenUrlDic removeObjectForKey:name];
    [_webViewResponseScheme removeObjectForKey:name];
}


-(void) updateBackgroundWebViewName:(NSString *)name transparent:(BOOL)transparent
{
    WebView *webView = [_webViewDic objectForKey:name];
    [webView setDrawsBackground:!transparent];
}

-(void) goBackWebViewName:(NSString *)name
{
    WebView *webView = [_webViewDic objectForKey:name];
    [webView goBack];
}

-(void) goForwardWebViewName:(NSString *)name
{
    WebView *webView = [_webViewDic objectForKey:name];
    [webView goForward];
}

-(void) webViewName:(NSString *)name loadHTMLString:(NSString *)htmlString baseURLString:(NSString *)baseURL
{
    WebView *webView = [_webViewDic objectForKey:name];
    [webView.mainFrame loadHTMLString:htmlString baseURL:[NSURL URLWithString:baseURL]];
}

-(void) webViewName:(NSString *)name EvaluatingJavaScript:(NSString *)javaScript shouldCallBack:(BOOL)callBack
{
    WebView *webView = [_webViewDic objectForKey:name];
    NSString *result = [webView stringByEvaluatingJavaScriptFromString:javaScript];
    if (callBack) {
        _CallMethod([name UTF8String], "EvalJavaScriptFinished", [result UTF8String]);
    }
}

-(NSString *) webViewGetUserAgent:(NSString *)name {
    WebView *webView = [_webViewDic objectForKey:name];
    return [webView stringByEvaluatingJavaScriptFromString:@"window.navigator.userAgent"];
}

-(void) webViewSetUserAgent:(NSString *)userAgent {
    self.customUserAgent = userAgent;
}

-(NSString *) webViewNameGetCurrentUrl:(NSString *)name {
    WebView *webView = [_webViewDic objectForKey:name];
    return [webView mainFrameURL] ?: @"";
}

-(NSString *) webViewName:(WebView *)webView
{
    NSString *webViewName = [[_webViewDic allKeysForObject:webView] lastObject];
    if (!webViewName) {
        NSLog(@"Did not find the webview: %@",webViewName);
    }
    return webViewName;
}

-(void) addWebViewName:(NSString *)name urlScheme:(NSString *)scheme {
    @synchronized(name) {
        NSMutableArray *schemes = [NSMutableArray arrayWithArray:[_webViewResponseScheme objectForKey:name]];
        if (![schemes containsObject:scheme]) {
            [schemes addObject:scheme];
            [_webViewResponseScheme setObject:schemes forKey:name];
        }
    }
}

-(void) removeWebViewName:(NSString *)name urlScheme:(NSString *)scheme {
    @synchronized(name) {
        NSMutableArray *schemes = [NSMutableArray arrayWithArray:[_webViewResponseScheme objectForKey:name]];
        if ([schemes containsObject:scheme]) {
            [schemes removeObject:scheme];
            [_webViewResponseScheme setObject:schemes forKey:name];
        }
    }
}

- (void)webView:(WebView *)sender decidePolicyForNavigationAction:(NSDictionary *)actionInformation request:(NSURLRequest *)request frame:(WebFrame *)frame decisionListener:(id<WebPolicyDecisionListener>)listener
{
    NSString *webViewName = [self webViewName:sender];
    NSArray * schemes = [_webViewResponseScheme objectForKey:webViewName];
    
    __block BOOL canResponse = NO;
    [schemes enumerateObjectsUsingBlock:^(NSString *scheme, NSUInteger idx, BOOL *stop) {
        if ([[request.URL absoluteString] rangeOfString:[scheme stringByAppendingString:@"://"]].location == 0) {
            canResponse = YES;
            *stop = YES;
        }
    }];
    
    if (canResponse) {
        NSString *rawMessage = [NSString stringWithFormat:@"%@",request.URL];
        _CallMethod([webViewName UTF8String], "ReceivedMessage", [rawMessage UTF8String]);
        [listener ignore];
    } else {
		[listener use];
    }
}

- (void)webView:(WebView *)sender didStartProvisionalLoadForFrame:(WebFrame *)frame
{
    if (frame == sender.mainFrame) {
        NSString *webViewName = [self webViewName:sender];
        [_webViewCurrenUrlDic setObject:sender.mainFrameURL forKey:webViewName];
        _CallMethod([webViewName UTF8String], "LoadBegin",[sender.mainFrameURL UTF8String]);
    }
}

- (void)webView:(WebView *)sender didFailLoadWithError:(NSError *)error forFrame:(WebFrame *)frame
{
    if (frame == sender.mainFrame) {
        NSString *webViewName = [self webViewName:sender];
        _CallMethod([webViewName UTF8String], "LoadComplete", [error.localizedDescription UTF8String]);
    }
}

- (void)webView:(WebView *)sender didFailProvisionalLoadWithError:(NSError *)error forFrame:(WebFrame *)frame
{
    if (frame == sender.mainFrame) {
        NSString *webViewName = [self webViewName:sender];
        _CallMethod([webViewName UTF8String], "LoadComplete", [error.localizedDescription UTF8String]);
    }
}

- (void)webView:(WebView *)sender didFinishLoadForFrame:(WebFrame *)frame
{
    if (frame == sender.mainFrame) {
//        NSLog(@"%@ didFinishLoadForFrame",sender);
        NSString *webViewName = [self webViewName:sender];
        _CallMethod([webViewName UTF8String], "LoadComplete", "");
    }
}

- (void)update:(int)x y:(int)y deltaY:(float)deltaY buttonDown:(BOOL)buttonDown buttonPress:(BOOL)buttonPress buttonRelease:(BOOL)buttonRelease keyPress:(BOOL)keyPress keyCode:(unsigned char)keyCode keyChars:(NSString *)keyChars textureId:(int)tId webViewName:(NSString *)name
{
    WebView *webView = [_webViewDic objectForKey:name];
//    NSLog(@"update WebView %@",webView);
	NSView *view = [[[webView mainFrame] frameView] documentView];
	NSGraphicsContext *context = [NSGraphicsContext currentContext];
	NSEvent *event;
    
	if (buttonDown) {
		if (buttonPress) {
			event = [NSEvent mouseEventWithType:NSLeftMouseDown
                                       location:NSMakePoint(x, y) modifierFlags:kNilOptions
                                      timestamp:GetCurrentEventTime() windowNumber:0
                                        context:context eventNumber:kNilOptions clickCount:1 pressure:1.0];
			[view mouseDown:event];
		} else {
			event = [NSEvent mouseEventWithType:NSLeftMouseDragged
                                       location:NSMakePoint(x, y) modifierFlags:kNilOptions
                                      timestamp:GetCurrentEventTime() windowNumber:0
                                        context:context eventNumber:kNilOptions clickCount:0 pressure:1.0];
			[view mouseDragged:event];
		}
	} else if (buttonRelease) {
		event = [NSEvent mouseEventWithType:NSLeftMouseUp
                                   location:NSMakePoint(x, y) modifierFlags:kNilOptions
                                  timestamp:GetCurrentEventTime() windowNumber:0
                                    context:context eventNumber:kNilOptions clickCount:0 pressure:1.0];
		[view mouseUp:event];
	}
    
	if (keyPress) {
		event = [NSEvent keyEventWithType:NSKeyDown
                                 location:NSMakePoint(x, y) modifierFlags:kNilOptions
                                timestamp:GetCurrentEventTime() windowNumber:0
                                  context:context
                               characters:keyChars
              charactersIgnoringModifiers:keyChars
                                isARepeat:NO keyCode:(unsigned short)keyCode];
		[view keyDown:event];
	}
    
	if (deltaY != 0) {
		CGEventRef cgEvent = CGEventCreateScrollWheelEvent(NULL, kCGScrollEventUnitLine, 1, deltaY * 3, 0);
		NSEvent *scrollEvent = [NSEvent eventWithCGEvent:cgEvent];
		CFRelease(cgEvent);
		[view scrollWheel:scrollEvent];
	}
    
    NSBitmapImageRep *bitmap = [_webViewDic objectForKey:name];
    @synchronized(bitmap) {
//        NSLog(@"NSBitmapImageRep %@",bitmap);
        bitmap = [webView bitmapImageRepForCachingDisplayInRect:[webView visibleRect]];
//        NSLog(@"New NSBitmapImageRep %@",bitmap);
        if (bitmap) {
            [_webViewBitMapDic setObject:bitmap forKey:name];
        }
        [_webViewTextureIdDic setObject:[NSNumber numberWithInt:tId] forKey:name];
//        NSLog(@"Set textureId %d",tId);
        [webView cacheDisplayInRect:[webView visibleRect] toBitmapImageRep:bitmap];
	}
}

- (void)render:(NSString *)webViewName
{
//    NSLog(@"render webview name: %@", webViewName);
    NSBitmapImageRep *bitmap = [_webViewBitMapDic objectForKey:webViewName];
//    NSLog(@"render bitmap: %@", bitmap);
    int textureId = (int)[[_webViewTextureIdDic objectForKey:webViewName] integerValue];
//    NSLog(@"render textureId: %d", textureId);
	@synchronized(bitmap) {
		if (bitmap) {
            NSInteger samplesPerPixel = [bitmap samplesPerPixel];
            int rowLength = 0;
            int unpackAlign = 0;
            glGetIntegerv(GL_UNPACK_ROW_LENGTH, &rowLength);
            glGetIntegerv(GL_UNPACK_ALIGNMENT, &unpackAlign);
            glPixelStorei(GL_UNPACK_ROW_LENGTH,
                          (int)[bitmap bytesPerRow] / samplesPerPixel);
            glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
            glBindTexture(GL_TEXTURE_2D, textureId);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            
            if (![bitmap isPlanar] &&
                (samplesPerPixel == 3 || samplesPerPixel == 4)) {
                glTexImage2D(GL_TEXTURE_2D,
                             0,
                             samplesPerPixel == 4 ? GL_RGBA8 : GL_RGB8,
                             (int)[bitmap pixelsWide],
                             (int)[bitmap pixelsHigh],
                             0,
                             samplesPerPixel == 4 ? GL_RGBA : GL_RGB,
                             GL_UNSIGNED_BYTE,
                             [bitmap bitmapData]);
            }
            glPixelStorei(GL_UNPACK_ROW_LENGTH, rowLength);
            glPixelStorei(GL_UNPACK_ALIGNMENT, unpackAlign);
        }
	}
}

-(void) receivedRenderEvent:(NSInteger)eventID {
//    NSLog(@"Receiving render event: %d", eventID);
    for (WebView *webView in [_webViewDic allValues]) {
        if ([[NSValue valueWithPointer:(void *)eventID] isEqualToValue:[NSValue valueWithPointer:webView]]) {
            [self render:[self webViewName:webView]];
        }
    }
}

-(WebView *) webViewWithName:(NSString *)name {
    return [_webViewDic objectForKey:name];
}
@end


// Helper method to create C string copy
NSString* UniWebViewMakeNSString (const char* string)
{
	if (string) {
		return [NSString stringWithUTF8String: string];
    } else {
		return [NSString stringWithUTF8String: ""];
    }
}

char* UniWebViewMakeCString(NSString *str)
{
    const char* string = [str UTF8String];
	if (string == NULL) {
		return NULL;
    }
    
	char* res = (char*)malloc(strlen(string) + 1);
	strcpy(res, string);
	return res;
}

extern "C" {
	void _UniWebViewInit(const char *name, int top, int left, int bottom, int right, int screenWidth, int screenHeight);
	void _UniWebViewChangeSize(const char *name, int top, int left, int bottom, int right, int screenWidth, int screenHeight);
	void _UniWebViewLoad(const char *name, const char *url);
    void _UniWebViewReload(const char *name);
    void _UniWebViewStop(const char *name);
	void _UniWebViewShow(const char *name);
	void _UniWebViewDismiss(const char *name);
    void _UniWebViewCleanCache(const char *name);
    void _UniWebViewCleanCookie(const char *name, const char *key);
    void _UniWebViewDestroy(const char *name);
    void _UniWebViewTransparentBackground(const char *name, BOOL transparent);
    void _UniWebViewGoBack(const char *name);
    void _UniWebViewGoForward(const char *name);
    void _UniWebViewLoadHTMLString(const char *name, const char *html, const char *baseUrl);
    
    void _UniWebViewInputEvent(const char *name, int x, int y, float deltaY,
                               BOOL buttonDown, BOOL buttonPress, BOOL buttonRelease,
                               BOOL keyPress, unsigned char keyCode, const char *keyChars, int textureId);
    void UnityRenderEvent(int eventID);
    void * _UniWebViewGetIntPtr(const char *name);
    void _UniWebViewEvaluatingJavaScript(const char *name, const char *javascript, BOOL callback);
    const char * _UniWebViewGetCurrentUrl(const char *name);
    void _UniWebViewAddUrlScheme(const char *name, const char *scheme);
    void _UniWebViewRemoveUrlScheme(const char *name, const char *scheme);
    const char * _UniWebViewGetUserAgent(const char *name);
    void _UniWebViewSetUserAgent(const char *userAgent);
}

void _UniWebViewInit(const char *name, int top, int left, int bottom, int right, int screenWidth, int screenHeight)
{
    NSEdgeInsets insets = NSEdgeInsetsMake(top, left, bottom, right);
    CGSize size = CGSizeMake(screenWidth, screenHeight);
    [[UniWebViewManager sharedManager] addManagedWebViewName:UniWebViewMakeNSString(name)
                                                      insets:insets
                                                  screenSize:size];
}

void _UniWebViewChangeSize(const char *name, int top, int left, int bottom, int right, int screenWidth, int screenHeight)
{
    NSEdgeInsets insets = NSEdgeInsetsMake(top, left, bottom, right);
    CGSize size = CGSizeMake(screenWidth, screenHeight);
    [[UniWebViewManager sharedManager] changeWebViewName:UniWebViewMakeNSString(name)
                                                  insets:insets
                                              screenSize:size];
}

void _UniWebViewLoad(const char *name, const char *url)
{
    [[UniWebViewManager sharedManager] webviewName:UniWebViewMakeNSString(name)
                                      beginLoadURL:UniWebViewMakeNSString(url)];
}

void _UniWebViewReload(const char *name)
{
    [[UniWebViewManager sharedManager] webViewNameReload:UniWebViewMakeNSString(name)];
}

void _UniWebViewStop(const char *name)
{
    [[UniWebViewManager sharedManager] webViewNameStop:UniWebViewMakeNSString(name)];
}

void _UniWebViewShow(const char *name) {
    [[UniWebViewManager sharedManager] webViewName:UniWebViewMakeNSString(name)
                                              show:YES];
}

void _UniWebViewDismiss(const char *name) {
    [[UniWebViewManager sharedManager] webViewName:UniWebViewMakeNSString(name)
                                              show:NO];
}

void _UniWebViewCleanCache(const char *name) {
    [[UniWebViewManager sharedManager] webViewNameCleanCache:UniWebViewMakeNSString(name)];
}

void _UniWebViewCleanCookie(const char *name, const char *key) {
    [[UniWebViewManager sharedManager] webViewNameCleanCookie:UniWebViewMakeNSString(name) forKey:UniWebViewMakeNSString(key)];
}

void _UniWebViewDestroy(const char *name) {
    [[UniWebViewManager sharedManager] removeWebViewName:UniWebViewMakeNSString(name)];
}

void _UniWebViewTransparentBackground(const char *name, BOOL transparent) {
    [[UniWebViewManager sharedManager] updateBackgroundWebViewName:UniWebViewMakeNSString(name) transparent:transparent];
}

void _UniWebViewGoBack(const char *name) {
    [[UniWebViewManager sharedManager] goBackWebViewName:UniWebViewMakeNSString(name)];
}

void _UniWebViewGoForward(const char *name) {
    [[UniWebViewManager sharedManager] goForwardWebViewName:UniWebViewMakeNSString(name)];
}

void _UniWebViewLoadHTMLString(const char *name, const char *html, const char *baseUrl) {
    [[UniWebViewManager sharedManager] webViewName:UniWebViewMakeNSString(name)
                                    loadHTMLString:UniWebViewMakeNSString(html)
                                     baseURLString:UniWebViewMakeNSString(baseUrl)];
}

void UnityRenderEvent(int eventID) {
    @autoreleasepool {
        [[UniWebViewManager sharedManager] receivedRenderEvent:eventID];
    }
}

void _UniWebViewInputEvent(const char *name, int x, int y, float deltaY,
                           BOOL buttonDown, BOOL buttonPress, BOOL buttonRelease,
                           BOOL keyPress, unsigned char keyCode, const char *keyChars, int textureId) {
    [[UniWebViewManager sharedManager] update:x y:y deltaY:deltaY buttonDown:buttonDown buttonPress:buttonPress buttonRelease:buttonRelease keyPress:keyPress keyCode:keyCode keyChars:UniWebViewMakeNSString(keyChars) textureId:textureId webViewName:UniWebViewMakeNSString(name)];
}

void *_UniWebViewGetIntPtr(const char *name) {
    return (void *)[[UniWebViewManager sharedManager] webViewWithName:UniWebViewMakeNSString(name)];
}

const char *_UniWebViewGetCurrentUrl(const char *name) {
    return UniWebViewMakeCString([[UniWebViewManager sharedManager] webViewNameGetCurrentUrl:UniWebViewMakeNSString(name)]);
}

void _UniWebViewEvaluatingJavaScript(const char *name, const char *javascript, BOOL callback) {
    NSString *webViewName = UniWebViewMakeNSString(name);
    NSString *jsString = UniWebViewMakeNSString(javascript);
//    NSLog(@"webViewName:%@, eval js:%@",webViewName,jsString);
    [[UniWebViewManager sharedManager] webViewName:webViewName EvaluatingJavaScript:jsString shouldCallBack:callback];
}

void _UniWebViewAddUrlScheme(const char *name, const char *scheme) {
    [[UniWebViewManager sharedManager] addWebViewName:UniWebViewMakeNSString(name)
                                            urlScheme:UniWebViewMakeNSString(scheme)];
}

void _UniWebViewRemoveUrlScheme(const char *name, const char *scheme) {
    [[UniWebViewManager sharedManager] removeWebViewName:UniWebViewMakeNSString(name)
                                               urlScheme:UniWebViewMakeNSString(scheme)];
}

const char * _UniWebViewGetUserAgent(const char *name) {
    return UniWebViewMakeCString([[UniWebViewManager sharedManager] webViewGetUserAgent:UniWebViewMakeNSString(name)]);
}

void _UniWebViewSetUserAgent(const char *userAgent) {
    [[UniWebViewManager sharedManager] webViewSetUserAgent:UniWebViewMakeNSString(userAgent)];
}

